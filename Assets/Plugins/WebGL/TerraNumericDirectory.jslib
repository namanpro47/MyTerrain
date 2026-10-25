mergeInto(LibraryManager.library, {
  TerraNumericDirectory: function(receiverPointer, requestPointer) {
    var receiver = UTF8ToString(receiverPointer), input, database = null, finished = false;
    function reply(ok, error, directory, definition, save) {
      if (finished) return;
      finished = true;
      if (database) database.close();
      SendMessage(receiver, 'OnNumericDirectoryReply', JSON.stringify({
        operationId: input && input.operationId, ok: ok, error: error || '',
        directoryJson: directory || '', definitionJson: definition || '', saveJson: save || ''
      }));
    }
    function hash(value) { return typeof value === 'string' && /^[a-f0-9]{64}$/.test(value); }
    function text(value) { return typeof value === 'string' && value.length > 0; }
    function escaped(value) { return encodeURIComponent(value).replace(/[!'()*]/g, function(c) { return '%' + c.charCodeAt(0).toString(16).toUpperCase(); }); }
    function code(error) { return error && error.name === 'QuotaExceededError' ? 'quota-exceeded' : 'storage-unavailable'; }
    function directory(value) {
      var d = JSON.parse(value), seen = Object.create(null);
      if (!d || d.schemaVersion !== 1 || d.townSchemaVersion !== 3 || d.worldPackId !== input.worldPackId
          || d.modelFamily !== 'annual-numeric-v4' || !Number.isSafeInteger(d.sequence) || d.sequence < 1
          || !hash(d.directoryHash) || !text(d.activeRunId) || !text(d.operationId) || !hash(d.operationHash)
          || !Array.isArray(d.runs) || d.runs.length === 0) throw new Error('directory');
      d.runs.forEach(function(r) {
        if (!r || !text(r.runId) || !hash(r.definitionHash) || seen[r.runId]) throw new Error('entry');
        seen[r.runId] = true;
      });
      if (!seen[d.activeRunId]) throw new Error('active');
      return d;
    }
    function record(value, expectedHash) {
      if (!value || typeof value.json !== 'string' || !hash(value.hash)
          || (expectedHash && value.hash !== expectedHash)) throw new Error('record');
      return value;
    }
    function rules(value) {
      var d = JSON.parse(value);
      if (!d || d.schemaVersion !== 3 || d.worldPackId !== input.worldPackId) throw new Error('definition');
      return d;
    }
    function save(value, entry) {
      var s = JSON.parse(value);
      if (!s || s.schemaVersion !== 1 || !Number.isSafeInteger(s.sequence) || s.sequence < 1 || !hash(s.saveHash)
          || !s.town || s.town.runId !== entry.runId || s.town.definitionHash !== entry.definitionHash
          || !Array.isArray(s.narration)) throw new Error('save');
      return s;
    }
    var candidate, entry, initial, definitionKey, runKey, writing;
    try {
      input = JSON.parse(UTF8ToString(requestPointer));
      if (!input || !text(input.operationId) || !text(input.worldPackId)
          || input.key !== 'myterrain.numeric-directory.v1/' + escaped(input.worldPackId) + '/schema-3/annual-numeric-v4'
          || ['read', 'read-run', 'create', 'select'].indexOf(input.operation) < 0) throw new Error('request');
      writing = input.operation === 'create' || input.operation === 'select';
      if (input.operation === 'read-run' && !text(input.runId)) throw new Error('run');
      if (writing) {
        if (input.expectedHash && !hash(input.expectedHash)) throw new Error('expected');
        candidate = directory(input.directoryJson);
        entry = candidate.runs.filter(function(r) { return r.runId === candidate.activeRunId; })[0];
        if (input.operation === 'create') {
          rules(input.definitionJson); initial = save(input.saveJson, entry);
          if (initial.sequence !== 1 || !Array.isArray(initial.town.history) || initial.town.history.length !== 0) throw new Error('initial');
        } else if (!hash(input.expectedSaveHash)) throw new Error('target');
      }
    } catch (_) { reply(false, 'invalid-request'); return; }
    var open;
    try { open = indexedDB.open('MyTerrainNumericSavesV1', 2); }
    catch (error) { reply(false, code(error)); return; }
    open.onupgradeneeded = function() {
      ['runs', 'definitions', 'directories'].forEach(function(name) {
        if (!open.result.objectStoreNames.contains(name)) open.result.createObjectStore(name);
      });
    };
    open.onerror = function() { reply(false, code(open.error)); };
    open.onblocked = function() { reply(false, 'storage-blocked'); };
    open.onsuccess = function() {
      database = open.result;
      if (finished) { database.close(); return; }
      database.onversionchange = function() { database.close(); };
      var transaction;
      try {
        transaction = writing ? database.transaction(['directories', 'definitions', 'runs'], 'readwrite', { durability: 'strict' })
          : database.transaction(['directories', 'definitions', 'runs'], 'readonly');
      } catch (error) { reply(false, code(error)); return; }
      var failure, outputDirectory = '', outputDefinition = '', outputSave = '';
      function abort(reason) { failure = reason; transaction.abort(); }
      transaction.onerror = function() { failure = failure || code(transaction.error); };
      transaction.onabort = function() { reply(false, failure || code(transaction.error)); };
      transaction.oncomplete = function() { reply(true, '', outputDirectory, outputDefinition, outputSave); };
      var directories = transaction.objectStore('directories'), definitions = transaction.objectStore('definitions'), runs = transaction.objectStore('runs');
      var request;
      try { request = directories.get(input.key); }
      catch (error) { abort(code(error)); return; }
      request.onsuccess = function() {
        var old = request.result, previous;
        try { if (old !== undefined) { record(old); previous = directory(old.json); if (old.hash !== previous.directoryHash) throw new Error('hash'); } }
        catch (_) { abort('corrupt-directory'); return; }
        if (writing) {
          if ((old === undefined ? null : old.hash) !== (input.expectedHash || null)) { abort('write-conflict'); return; }
          var before = previous ? previous.runs : [], after = candidate.runs;
          if (candidate.sequence !== (previous ? previous.sequence + 1 : 1)
              || after.length !== before.length + (input.operation === 'create' ? 1 : 0)
              || before.some(function(r, i) { return r.runId !== after[i].runId || r.definitionHash !== after[i].definitionHash; })
              || (input.operation === 'create' && (before.some(function(r) { return r.runId === entry.runId; }) || after[after.length - 1].runId !== entry.runId))) {
            abort('invalid-transition'); return;
          }
        } else {
          if (!previous) { if (input.operation === 'read-run') abort('missing-directory'); return; }
          var id = input.operation === 'read-run' ? input.runId : previous.activeRunId;
          entry = previous.runs.filter(function(r) { return r.runId === id; })[0];
          if (!entry) { abort('missing-run'); return; }
          outputDirectory = old.json;
        }
        definitionKey = 'myterrain.numeric-definition.v1/' + escaped(input.worldPackId) + '/schema-3/' + entry.definitionHash;
        runKey = 'myterrain.numeric.v1/' + escaped(input.worldPackId) + '/schema-3/' + entry.definitionHash + '/' + escaped(entry.runId);
        var definitionRead;
        try { definitionRead = definitions.get(definitionKey); }
        catch (error) { abort(code(error)); return; }
        definitionRead.onsuccess = function() {
          var storedDefinition = definitionRead.result;
          if (input.operation === 'create') {
            try { if (storedDefinition !== undefined) { record(storedDefinition, entry.definitionHash); rules(storedDefinition.json); if (storedDefinition.json !== input.definitionJson) throw new Error('immutable'); } }
            catch (_) { abort('definition-conflict'); return; }
          } else {
            try { record(storedDefinition, entry.definitionHash); rules(storedDefinition.json); }
            catch (_) { abort('missing-or-corrupt-definition'); return; }
          }
          var runRead;
          try { runRead = runs.get(runKey); }
          catch (error) { abort(code(error)); return; }
          runRead.onsuccess = function() {
            var storedRun = runRead.result, parsed;
            if (input.operation === 'create') {
              if (storedRun !== undefined) { abort('run-already-exists'); return; }
            } else {
              try { record(storedRun); parsed = save(storedRun.json, entry); if (parsed.saveHash !== storedRun.hash) throw new Error('hash'); }
              catch (_) { abort('missing-or-corrupt-run'); return; }
              if (input.operation === 'select' && parsed.saveHash !== input.expectedSaveHash) { abort('run-write-conflict'); return; }
            }
            if (!writing) { outputDefinition = storedDefinition.json; outputSave = storedRun.json; return; }
            try {
              if (input.operation === 'create') {
                if (storedDefinition === undefined) definitions.add({ hash: entry.definitionHash, json: input.definitionJson }, definitionKey);
                runs.add({ hash: initial.saveHash, json: input.saveJson }, runKey);
              }
              directories.put({ hash: candidate.directoryHash, json: input.directoryJson }, input.key);
            } catch (error) { abort(code(error)); }
          };
        };
      };
    };
  }
});
