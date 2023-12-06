using Eva.Views;
using System.Diagnostics;

namespace Eva
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private void AboutItem_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (Navigation.NavigationStack.Where(x => x is AboutPage).Count() > 0)
                    return;
                Navigation.PushAsync(new AboutPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);
            }
        }
    }
}
