using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppleCount
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FinishPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public FinishPopup()
        {
            InitializeComponent();
        }
    }
}