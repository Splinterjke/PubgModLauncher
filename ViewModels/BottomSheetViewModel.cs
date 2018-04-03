using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubgMod.ViewModels
{
    public class BottomSheetViewModel : Screen
    {
        public string Message { get; set; }
        public string ActionContent { get; private set; } = "ЗАКРЫТЬ";
        public bool IsDisplayed { get; set; }

        public void CloseBottomSheet()
        {
            IsDisplayed = false;
        }

        public void ShowBottomSheet()
        {
            IsDisplayed = true;
        }
    }
}
