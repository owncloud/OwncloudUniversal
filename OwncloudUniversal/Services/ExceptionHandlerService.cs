using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            var exception = e.Exception;
            Debug.WriteLine(exception.GetType());
            e.Handled = true;
            MessageDialog dia = new MessageDialog(App.ResourceLoader.GetString("UnhandledExceptionMessage"));
            dia.Title = App.ResourceLoader.GetString("Ooops");
            if ((uint)exception.HResult == 0x80072EE7)//server not found
            {
                dia.Content = App.ResourceLoader.GetString("ServerNotFound");
            }

            else if (exception.GetType() == typeof(WebDavException))//-2146233088
            {
                var ex = (WebDavException)exception;
                switch (ex.HttpStatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        dia.Content = App.ResourceLoader.GetString("UnauthorizedMessage");
                        break;
                    case HttpStatusCode.NotFound:
                        dia.Content = App.ResourceLoader.GetString("NotFoundMessage");
                        break;
                    case HttpStatusCode.InternalServerError:
                        dia.Content = App.ResourceLoader.GetString("InternalServerErrorMessage");
                        break;
                    default:
                        dia.Content = App.ResourceLoader.GetString("ServiceUnavailableMessage");
                        break;
                }
            }
            await LogHelper.Write($"{e.Exception.GetType()}: {e.Message} \r\n{exception.StackTrace}");
            IndicatorService.GetDefault().HideBar();
            await dia.ShowAsync();
        }
    }
}
