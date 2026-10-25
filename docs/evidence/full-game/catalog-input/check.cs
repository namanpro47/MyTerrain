// Actual runtime HUD + Input System routing; preserve the live annual state.
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
if(main==null)throw new System.Exception("Open TerraMain and enter Play mode first");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var hud=(Terra.UI.TerraHud)main.GetType().GetField("hud",flags).GetValue(main);
var session=(Terra.Gameplay.AnnualTownSession)main.GetType().GetField("annual",flags).GetValue(main);
var type=typeof(Terra.UI.TerraHud);var mode=type.GetField("catalogKeyboard",flags);
var pointer=main.GetType().GetMethod("Pointer",flags);
string before=UnityEngine.JsonUtility.ToJson(session.State);
hud.CloseModal();mode.SetValue(hud,false);
var oldFocusPolicy=UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode;
var oldBackground=UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior;
UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
var oldUpdateMode=UnityEngine.InputSystem.InputSystem.settings.updateMode;
UnityEngine.InputSystem.InputSystem.settings.updateMode=UnityEngine.InputSystem.InputSettings.UpdateMode.ProcessEventsManually;
var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
UnityEngine.InputSystem.InputSystem.EnableDevice(keyboard);
foreach(var key in keyboard.allKeys){bool prime=key.wasPressedThisFrame;}
int checks=0;var cases=new System.Collections.Generic.List<string>();
void Check(bool value,string label){checks++;if(!value)throw new System.Exception(label);cases.Add(label);}
bool Press(params UnityEngine.InputSystem.Key[] keys)
{
 bool handled=false;bool sampled=false;
 void Sample(){handled=hud.KeyboardInput(keyboard);sampled=true;}
 UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(keys),-1);
 UnityEngine.InputSystem.InputSystem.onAfterUpdate+=Sample;
 try{UnityEngine.InputSystem.InputSystem.Update();}
 finally{UnityEngine.InputSystem.InputSystem.onAfterUpdate-=Sample;}
 UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
 UnityEngine.InputSystem.InputSystem.Update();
 if(!sampled)throw new System.Exception("Input update callback did not run");
 return handled;
}
bool Mode()=> (bool)mode.GetValue(hud);
try
{
 Check(Press(UnityEngine.InputSystem.Key.B)&&Mode(),"B enters catalog navigation");
 hud.PointerHover(new UnityEngine.Vector2(-50,-50));
 Check(Mode(),"Hover does not cancel keyboard navigation");
 Check(Press(UnityEngine.InputSystem.Key.Tab)&&Mode(),"Catalog Tab stays inside catalog");
 pointer.Invoke(main,new object[]{0,new UnityEngine.Vector2(-50,-50),false});
 Check(!Mode(),"Actual pointer-down routing exits catalog navigation");
 pointer.Invoke(main,new object[]{3,new UnityEngine.Vector2(-50,-50),false});
 Check(!Press(UnityEngine.InputSystem.Key.Z),"Undo is forwarded after pointer use");
 foreach(var key in new[]{UnityEngine.InputSystem.Key.Z,UnityEngine.InputSystem.Key.Y})
 {
  Check(Press(UnityEngine.InputSystem.Key.B)&&Mode(),"Reenter catalog before "+key);
  Check(!Press(key)&&!Mode(),key+" exits catalog and reaches gameplay routing");
 }
 Press(UnityEngine.InputSystem.Key.B);
 Check(!Press(UnityEngine.InputSystem.Key.LeftShift,UnityEngine.InputSystem.Key.Z)&&!Mode(),"Shift-Z also reaches redo routing");
 Press(UnityEngine.InputSystem.Key.B);
 Check(Press(UnityEngine.InputSystem.Key.LeftArrow)&&Mode(),"Catalog arrows stay contained");
 Check(Press(UnityEngine.InputSystem.Key.Escape)&&!Mode(),"Escape closes catalog navigation");
 Press(UnityEngine.InputSystem.Key.B);
 hud.AnnualDialog("Input isolation check","No action is submitted.","Close",()=>hud.CloseModal());
 Check(hud.ModalOpen,"Actual modal opened");
 Check(Press(UnityEngine.InputSystem.Key.Z)&&hud.ModalOpen,"Modal consumes Undo");
 Check(Press(UnityEngine.InputSystem.Key.Y)&&hud.ModalOpen,"Modal consumes Redo");
 Check(Press(UnityEngine.InputSystem.Key.Escape)&&!hud.ModalOpen,"Modal Escape closes without gameplay action");
 Check(!Press(UnityEngine.InputSystem.Key.Z)&&!Mode(),"Undo routing recovers after closing modal");
 Check(UnityEngine.JsonUtility.ToJson(session.State)==before,"Annual state byte-identical after input checks");
 return new{checks,cases,frame=UnityEngine.Time.frameCount,annual=main.Diagnostic};
}
finally
{
 hud.CloseModal();mode.SetValue(hud,false);
 UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
 UnityEngine.InputSystem.InputSystem.settings.updateMode=oldUpdateMode;
 UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=oldFocusPolicy;
 UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=oldBackground;
}
