"""Data/package negative cases and honest availability/reference-adapter checks."""
import copy
import hashlib
import json
from pathlib import Path
import shutil
import tempfile
import unittest

from validate_content import (ROOT, dto_fields, read_json, render_body, validate_entry,
                              validate_package)


class EducationContentTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.entries, cls.bindings = validate_package()
        cls.sources = {s['id'] for s in read_json(ROOT/'worlds/meadowbrook-v1/manifest.json')['sources']}
        source = (ROOT/'Assets/Terra/Scripts/Contracts/WorldContracts.cs').read_text()
        cls.entry_fields = dto_fields(source, 'ContentEntry')
        cls.choice_fields = dto_fields(source, 'ContentChoice')

    def test_frozen_fixture_intro_unchanged(self):
        self.assertEqual(hashlib.sha256((ROOT/'content/education/fixture-intro.json').read_bytes()).hexdigest(),
                         '77857f6c3961eda8135992b74b1facc5828a37769af3824f08d4410d819de643')

    def test_malformed_entry_fixtures(self):
        folder = Path(__file__).parent/'fixtures'
        for case in read_json(folder/'cases.json'):
            with self.subTest(fixture=case['file']), self.assertRaisesRegex(ValueError, case['error']):
                validate_entry(read_json(folder/case['file']), self.sources,
                               self.bindings['placeholders'], self.bindings['characterLimits'],
                               self.entry_fields, self.choice_fields)

    def test_duplicate_json_keys_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp)/'duplicate.json'
            path.write_text('{"id":"first","id":"hidden-overwrite"}')
            with self.assertRaisesRegex(ValueError, 'duplicate JSON key'):
                read_json(path)

    def render(self, id, values=None, guards=None):
        return render_body(id, self.entries, self.bindings, values or {}, guards or {})

    def test_pending_zero_never_becomes_runoff_measurement(self):
        values = {'baselineDischargedM3':0, 'dischargedM3':0}
        self.assertEqual(self.render('ending-water', values, {'matchedRunoffAvailable':False}),
                         self.entries['ending-water-unavailable']['body'])
        self.assertEqual(self.render('tool-water-swale'), self.entries['tool-water-swale-unavailable']['body'])
        self.assertEqual(self.render('disclosure-model'), self.entries['disclosure-unavailable']['body'])

    def test_available_zero_baseline_needs_no_division(self):
        text = self.render('ending-water', {'baselineDischargedM3':0, 'dischargedM3':0}, {'matchedRunoffAvailable':True})
        self.assertIn('baseline 0.0 m³; proposal 0.0 m³', text)
        self.assertNotIn('%', text)

    def test_one_missing_or_bad_number_uses_complete_fallback(self):
        for invalid in (None, float('nan'), float('inf'), -1, '0', False):
            with self.subTest(value=invalid):
                text = self.render('ending-water', {'baselineDischargedM3':7, 'dischargedM3':invalid}, {'matchedRunoffAvailable':True})
                self.assertEqual(text, self.entries['ending-water-unavailable']['body'])
                self.assertNotIn('{', text)

    def test_changed_result_changes_copy_and_preserves_negative_delta(self):
        values = {'production':12.5,'netProduction':-3,'spentCredits':9,'remainingCredits':51}
        text = self.render('ending-accounting', values, {'currentResultAvailable':True})
        self.assertIn('12.5 illustrative units', text)
        self.assertIn('-3.0 units', text)
        self.assertIn('spent: 9; remaining: 51', text)
        values['production'] = 18
        self.assertIn('18.0 illustrative units', self.render('ending-accounting', values, {'currentResultAvailable':True}))
        values['spentCredits'] = 9.5
        self.assertEqual(self.render('ending-accounting', values, {'currentResultAvailable':True}), self.entries['ending-accounting-unavailable']['body'])

    def test_unmet_goals_do_not_show_success_or_unsupported_actions(self):
        for id in ('ending-invitation','ending-continue','ending-retry'):
            self.assertIsNone(self.render(id))
        self.assertIsNotNone(self.render('ending-invitation', guards={'objectivesMet':True}))

    def test_every_template_renders_available_and_unavailable(self):
        values = {name:1234.5 if spec['format'] != 'integer' else 1234 for name,spec in self.bindings['placeholders'].items()}
        guards = {spec['guard']:True for spec in self.bindings['placeholders'].values()}
        guards.update({g:True for g in self.bindings['capabilityGuards'].values()})
        for id, entry in self.entries.items():
            with self.subTest(id=id):
                available = self.render(id, values, guards)
                self.assertNotIn('{', available)
                self.assertLessEqual(len(available), self.bindings['characterLimits']['renderedBody'])
                unavailable = self.render(id)
                if unavailable is not None:
                    self.assertNotIn('{', unavailable)

    def test_all_take_home_choices_are_unscored_and_have_cards(self):
        choices = self.entries['take-home-choice']['choices']
        self.assertTrue(all(not c['correct'] for c in choices))
        self.assertEqual(len(choices), len(self.bindings['choiceTargets']))

    def test_read_only_alternate_core_source_supports_phase3_and_rejects_unknown(self):
        phase3_reasons = [
            'Choose an editing action.', 'Unknown editing action.',
            'There are no proposals to clear.', 'There is no proposal to edit at this site.',
            'Choose a destination site.', 'Choose a different destination site.',
            'The destination already has a proposal. Remove it or choose another site.',
            'Not enough credits. Remove or undo a proposal to try this one.'
        ]
        baseline = (ROOT/'Assets/Terra/Scripts/Core/WorldStateStore.cs').read_text()
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp)/'WorldStateStore.cs'
            path.write_text(baseline + '\n' + '\n'.join('Invalid(' + json.dumps(r) + ');' for r in phase3_reasons))
            before = path.read_bytes()
            entries, bindings = validate_package(core_source=path)
            self.assertEqual(before, path.read_bytes())
            for reason in phase3_reasons:
                self.assertIn(bindings['reasonBindings'][reason], entries)
            path.write_text(path.read_text() + '\nInvalid("A future unmapped reason.");')
            with self.assertRaisesRegex(ValueError, 'Runtime reasons changed'):
                validate_package(core_source=path)

    def test_package_reference_and_question_failures(self):
        def missing_fallback(root):
            self.change_bindings(root, lambda b:b['fallbacks'].pop('ending-water'))
        def bad_path(root):
            self.change_bindings(root, lambda b:b['placeholders']['production'].update(path='WorldResult.imaginedYield'))
        def dangling_card(root):
            self.change_bindings(root, lambda b:b['choiceTargets'].update({'home-water':'unknown-card'}))
        def missing_intro(root):
            path=root/'worlds/meadowbrook-v1/scenario.json'
            data=read_json(path); data['introId']='missing-intro'; path.write_text(json.dumps(data))
        def two_correct(root):
            path=root/'content/education/meadowbrook-v1/runoff-transfer-v1.json'
            data=read_json(path); data['choices'][0]['correct']=True; path.write_text(json.dumps(data))
        def duplicate_entry(root):
            self.change_bindings(root, lambda b:b['entries'].append(b['entries'][0]))
        def new_reason(root):
            path=root/'Assets/Terra/Scripts/Core/WorldStateStore.cs'
            path.write_text(path.read_text()+'\n// fixture: Invalid("A new reason needs a mapping.")\n')
        def unsafe_runoff_guard(root):
            self.change_bindings(root, lambda b:b['placeholders']['dischargedM3'].update(guard='currentResultAvailable'))
        def wrong_baseline_instance(root):
            self.change_bindings(root, lambda b:b['placeholders']['baselineDischargedM3'].update(instance='current'))
        def wrong_runoff_field(root):
            self.change_bindings(root, lambda b:b['placeholders']['dischargedM3'].update(path='WorldResult.generatedM3'))
        def missing_current_instance(root):
            self.change_bindings(root, lambda b:b['placeholders']['dischargedM3'].pop('instance'))
        for mutation, message in [(unsafe_runoff_guard,'Runoff placeholder'),
                                  (wrong_baseline_instance,'Runoff placeholder'),
                                  (wrong_runoff_field,'Runoff placeholder'),
                                  (missing_current_instance,'Runoff placeholder'),(missing_fallback,'lacks whole-entry fallback'),(bad_path,'Unknown DTO'),
                                  (dangling_card,'Unknown Take It Home'),(missing_intro,'Scenario content'),
                                  (two_correct,'one reasoned best answer'),(duplicate_entry,'Duplicate entry path'),
                                  (new_reason,'Runtime reasons changed')]:
            with self.subTest(mutation=mutation.__name__), tempfile.TemporaryDirectory() as temp:
                root=Path(temp)
                shutil.copytree(ROOT/'content/education',root/'content/education')
                for relative in ['worlds/meadowbrook-v1/manifest.json','worlds/meadowbrook-v1/scenario.json',
                                 'worlds/meadowbrook-v1/world.json','Assets/Terra/Scripts/Contracts/WorldContracts.cs',
                                 'Assets/Terra/Scripts/Core/WorldStateStore.cs']:
                    dst=root/relative; dst.parent.mkdir(parents=True,exist_ok=True); shutil.copyfile(ROOT/relative,dst)
                mutation(root)
                with self.assertRaisesRegex(ValueError,message):
                    validate_package(root)

    @staticmethod
    def change_bindings(root, mutation):
        path=root/'content/education/bindings.json'
        data=read_json(path); mutation(data); path.write_text(json.dumps(data))


if __name__ == '__main__':
    unittest.main()
