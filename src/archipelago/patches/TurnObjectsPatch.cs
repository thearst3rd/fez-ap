using System.Reflection;
using FezEngine.Services.Scripting;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Services;
using MonoMod.RuntimeDetour;

/*
 * Prevents Gomez from being able to turn various types of pivotable objects if the Turn Objects ability has not been
 * unlocked in the Archipelago.
 */
namespace FEZAP.Archipelago
{
    public class TurnObjectsPatch : IFezapPatch
    {
        [ServiceDependency]
        public IPlayerManager PlayerManager { private get; set; }

        [ServiceDependency]
        public IDotService DotService { private get; set; }

        private Hook PivotStateSpinHook;
        private Hook ValveStateGrabOntoHook;
        private Hook TombstoneStateGrabOntoHook;

        private bool DotTalking = false;

        public void Init()
        {
            Type PivotState = typeof(PivotsHost).GetNestedType("PivotState", BindingFlags.NonPublic);
            PivotStateSpinHook = new Hook(PivotState.GetMethod("Spin"), TurnObjectsAllowedHooked);

            Type ValveState = typeof(ValvesBoltsTimeswitchesHost).GetNestedType("ValveState", BindingFlags.NonPublic);
            ValveStateGrabOntoHook = new Hook(ValveState.GetMethod("GrabOnto"), TurnObjectsAllowedHooked);

            Type TombstoneState = typeof(TombstonesHost).GetNestedType("TombstoneState", BindingFlags.NonPublic);
            TombstoneStateGrabOntoHook = new Hook(TombstoneState.GetMethod("GrabOnto"), TurnObjectsAllowedHooked);
        }

        private void TurnObjectsAllowedHooked(Action<object> original, object self)
        {
            if (ItemManager.ReceivedAbilityData.TurnObjects)
            {
                original(self);
                return;
            }

            if (!DotTalking)
            {
                DotTalking = true;
                DotService.Say("@You can't turn objects yet.", true, true).Ended = delegate { DotTalking = false; };
            }
        }

        public void Dispose()
        {
            PivotStateSpinHook.Dispose();
            ValveStateGrabOntoHook.Dispose();
            TombstoneStateGrabOntoHook.Dispose();
        }
    }
}
