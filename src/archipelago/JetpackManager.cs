using System.Reflection;
using FezEngine.Tools;
using FezGame;
using FezGame.Services;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;

namespace FEZAP.Archipelago
{
    public class JetpackManager(Game game) : GameComponent(game)
    {
        public static bool JetpackUnlocked = false;

        [ServiceDependency]
        public IGameStateManager GameState { private get; set; }

        private Hook GameWideCodesTestInputMethodHook;

        public override void Initialize()
        {
            Type GameWideCodes = typeof(Fez).Assembly.GetType("FezGame.Components.GameWideCodes");
            MethodInfo GameWideCodesTestInputMethod = GameWideCodes.GetMethod("TestInput", BindingFlags.NonPublic | BindingFlags.Instance);

            GameWideCodesTestInputMethodHook = new Hook(GameWideCodesTestInputMethod, GameWideCodesTestInputMethodHooked);
        }

        private void GameWideCodesTestInputMethodHooked(Action<object> original, object self)
        {
            bool origFinished32 = GameState.SaveData.Finished32;
            if (JetpackUnlocked)
                GameState.SaveData.Finished32 = true;
            original(self);
            GameState.SaveData.Finished32 = origFinished32;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            GameWideCodesTestInputMethodHook.Dispose();
        }
    }
}
