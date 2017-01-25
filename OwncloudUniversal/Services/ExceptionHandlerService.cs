using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.Web.Http;
using OwncloudUniversal.Shared;
using OwncloudUniversal.WebDav.Model;

namespace OwncloudUniversal.Services
{
    static class ExceptionHandlerService
    {
        public static  async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            MessageDialog dia = new MessageDialog(e.Message);
            dia.Title = App.ResourceLoader.GetString("Ooops");
            if ((uint)e.Exception.HResult == 0x80072EE7)//server not found
            {
                dia.Content = App.ResourceLoader.GetString("ServerNotFound");
            }

            else if (e.Exception.GetType() == typeof(WebDavException))//-2146233088
            {
                var ex = (WebDavException)e.Exception;
                if (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
                {
                    dia.Content = App.ResourceLoader.GetString("UnauthorizedMessage");
                }

                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    dia.Content = App.ResourceLoader.GetString("NotFoundMessage");
                }

                if (ex.HttpStatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    dia.Content = App.ResourceLoader.GetString("ServiceUnavailableMessage");
                }

                if (ex.HttpStatusCode == HttpStatusCode.InternalServerError)
                {
                    dia.Content = App.ResourceLoader.GetString("InternalServerErrorMessage");
                }

            }
            else
            {
                await LogHelper.Write($"{e.Exception.GetType()}: {e.Message} \r\n{e.Exception.StackTrace}");
            }
            IndicatorService.GetDefault().HideBar();
            await dia.ShowAsync();
        }
    }
}
