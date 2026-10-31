using System.Reflection;
using FezGame.Components;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

/*
 * When using the CodeInputScrambler, map the inputs being used onto their scrambled counterparts while building up the
 * input sequence, so that you need to know the scrambled inputs to enter the code.
 */
namespace FEZAP.Archipelago
{
    public class CodeInputPatch : IFezapPatch
    {
        private ILHook VolumesGrabInputHook;

        public void Init()
        {
            VolumesGrabInputHook = new ILHook(typeof(VolumesHost).GetMethod("GrabInput", BindingFlags.NonPublic | BindingFlags.Instance), il => {
                ILCursor cursor = new(il);

                cursor.GotoNext(MoveType.After, [ // Input.Add(codeInput); after loading `codeInput` but before calling `Add()`
                    i => i.MatchLdarg(0),
                    i => i.MatchLdfld("FezGame.Components.VolumesHost", "Input"),
                    i => i.MatchLdloc(0),
                ]);

                // Fudge `codeInput` (currently top of the stack) using the scramble function
                cursor.EmitDelegate(CodeInputScrambler.GetScrambledCode);
            });
        }

        public void Dispose()
        {
            VolumesGrabInputHook.Dispose();
        }
    }
}
