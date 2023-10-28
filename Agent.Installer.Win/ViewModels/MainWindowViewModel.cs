using Remotely.Agent.Installer.Win.Models;
using Remotely.Agent.Installer.Win.Services;
using Remotely.Agent.Installer.Win.Utilities;
using Remotely.Shared.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using Remotely.Shared;
using Remotely.Agent.Installer.Models;
using System.Configuration;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Management;
using Microsoft.AspNetCore.Http.Features;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Http;
using System.Net.Sockets;

namespace Remotely.Agent.Installer.Win.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly InstallerService _installer;
        private readonly EmbeddedServerDataReader _embeddedDataReader;
        private bool _createSupportShortcut;
        private string _headerMessage = "Install the service.";
        private bool _isReadyState = true;
        private bool _isServiceInstalled;

        private string _password;

        private string _organizationID;

        private string _serverUrl = string.Empty;

        private int _progress;

        private string _username= string.Empty;

        private string _statusMessage = "";

        private string RDP_SERVER = "https://staging-tuso.api.arcapps.org/tuso-api/rdpserver/";

        

        public MainWindowViewModel()
        {
            _installer = new InstallerService();
            _embeddedDataReader = new EmbeddedServerDataReader();

            CopyCommandLineArgs();

            ExtractEmbeddedServerData().Wait();

            AddExistingConnectionInfo();

        }


        public bool CreateSupportShortcut
        {
            get
            {
                return _createSupportShortcut;
            }
            set
            {
                _createSupportShortcut = value;
                FirePropertyChanged();
            }
        }

        public string HeaderMessage
        {
            get
            {
                return _headerMessage;
            }
            set
            {
                _headerMessage = value;
                FirePropertyChanged();
            }
        }

        public BitmapImage Icon { get; set; }
        public string InstallButtonText => IsServiceMissing ? "Install" : "Reinstall";

        public ICommand InstallCommand => new RelayCommand(async (param) => { await Install(null); });

        public bool IsProgressVisible => Progress > 0;

        public bool IsReadyState
        {
            get
            {
                return _isReadyState;
            }
            set
            {
                _isReadyState = value;
                FirePropertyChanged();
            }
        }

        public bool IsServiceInstalled
        {
            get
            {
                return _isServiceInstalled;
            }
            set
            {
                _isServiceInstalled = value;
                FirePropertyChanged();
                FirePropertyChanged(nameof(IsServiceMissing));
                FirePropertyChanged(nameof(InstallButtonText));
            }
        }

        public bool IsServiceMissing => !_isServiceInstalled;

        public ICommand SignInCommand => new RelayCommand(async (param) => { await SignIn(); });

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                FirePropertyChanged();
            }
        }

        public string ProductName { get; set; }

        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                FirePropertyChanged();
                FirePropertyChanged(nameof(IsProgressVisible));
            }
        }

        public string UserName
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                FirePropertyChanged();
            }
        }

        public string OrganizationID
        {
            get
            {
                return _organizationID;
            }
            set
            {
                _organizationID = value;
                FirePropertyChanged();
            }
        }

        public string ServerUrl
        {
            get
            {
                return _serverUrl;
            }
            set
            {
                _serverUrl = value?.TrimEnd('/');
                FirePropertyChanged();
            }
        }

        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                FirePropertyChanged();
            }
        }

        public SolidColorBrush TitleBackgroundColor { get; set; }
        public SolidColorBrush TitleButtonForegroundColor { get; set; }
        public SolidColorBrush TitleForegroundColor { get; set; }
        public ICommand UninstallCommand => new RelayCommand(async (param) => { await Uninstall(); });
        private string DeviceAlias { get; set; }
        private string DeviceGroup { get; set; }
        private string DeviceUuid { get; set; }
        public async Task Init()
        {
            _installer.ProgressMessageChanged += (sender, arg) =>
            {
                //StatusMessage = arg;
            };

            _installer.ProgressValueChanged += (sender, arg) =>
            {
                Progress = arg;
            };

            IsServiceInstalled = ServiceController.GetServices().Any(x => x.ServiceName == "Remotely_Service");
            if (IsServiceMissing)
            {
                HeaderMessage = $"Install the {ProductName} service.";
            }
            else
            {
                HeaderMessage = $"Modify the {ProductName} installation.";
            }

            CommandLineParser.VerifyArguments();

            if (CommandLineParser.CommandLineArgs.ContainsKey("install"))
            {
                await Install(null);
            }
            else if (CommandLineParser.CommandLineArgs.ContainsKey("uninstall"))
            {
                await Uninstall();
            }

            if (CommandLineParser.CommandLineArgs.ContainsKey("quiet"))
            {
                App.Current.Shutdown();
            }

            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(
                delegate { return true; }
            );
        }

        private void AddExistingConnectionInfo()
        {
            try
            {
                var connectionInfoPath = Path.Combine(
                Path.GetPathRoot(Environment.SystemDirectory),
                    "Program Files",
                    "Remotely",
                    "ConnectionInfo.json");

                if (File.Exists(connectionInfoPath))
                {
                    var serializer = new JavaScriptSerializer();
                    var connectionInfo = serializer.Deserialize<Microsoft.AspNetCore.Http.ConnectionInfo>(File.ReadAllText(connectionInfoPath));

                    /*if (string.IsNullOrWhiteSpace(OrganizationID))
                    {
                        OrganizationID = connectionInfo.OrganizationID;
                    }

                    if (string.IsNullOrWhiteSpace(ServerUrl))
                    {
                        ServerUrl = connectionInfo.Host;
                    }*/
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

        }

        private void ApplyBranding(BrandingInfo brandingInfo)
        {
            try
            {
                ProductName = "Remotely";

                if (!string.IsNullOrWhiteSpace(brandingInfo?.Product))
                {
                    ProductName = brandingInfo.Product;
                }

                TitleBackgroundColor = new SolidColorBrush(Color.FromRgb(
                    brandingInfo?.TitleBackgroundRed ?? 70,
                    brandingInfo?.TitleBackgroundGreen ?? 70,
                    brandingInfo?.TitleBackgroundBlue ?? 70));

                TitleForegroundColor = new SolidColorBrush(Color.FromRgb(
                   brandingInfo?.TitleForegroundRed ?? 29,
                   brandingInfo?.TitleForegroundGreen ?? 144,
                   brandingInfo?.TitleForegroundBlue ?? 241));

                TitleButtonForegroundColor = new SolidColorBrush(Color.FromRgb(
                   brandingInfo?.ButtonForegroundRed ?? 255,
                   brandingInfo?.ButtonForegroundGreen ?? 255,
                   brandingInfo?.ButtonForegroundBlue ?? 255));

                TitleBackgroundColor.Freeze();
                TitleForegroundColor.Freeze();
                TitleButtonForegroundColor.Freeze();

                Icon = GetBitmapImageIcon(brandingInfo);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        private bool CheckIsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var result = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!result)
            {
                MessageBoxEx.Show("Elevated privileges are required.  Please restart the installer using 'Run as administrator'.", "Elevation Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private bool CheckParams()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            {
                Logger.Write("Username or Password param is missing.  Unable to sign in.");
                MessageBoxEx.Show("Required settings are missing.  Please enter a username and password.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void CopyCommandLineArgs()
        {
            if (CommandLineParser.CommandLineArgs.TryGetValue("username", out var userName))
            {
                UserName = userName;
            }

            if (CommandLineParser.CommandLineArgs.TryGetValue("password", out var password))
            {
                Password = password;
            }

            if (CommandLineParser.CommandLineArgs.TryGetValue("devicegroup", out var deviceGroup))
            {
                DeviceGroup = deviceGroup;
            }

            if (CommandLineParser.CommandLineArgs.TryGetValue("devicealias", out var deviceAlias))
            {
                DeviceAlias = deviceAlias;
            }

            if (CommandLineParser.CommandLineArgs.TryGetValue("deviceuuid", out var deviceUuid))
            {
                DeviceUuid = deviceUuid;
            }

            if (CommandLineParser.CommandLineArgs.ContainsKey("supportshortcut"))
            {
                CreateSupportShortcut = true;
            }
        }

        private async Task ExtractEmbeddedServerData()
        {

            try
            {
                var filePath = Process.GetCurrentProcess()?.MainModule?.FileName;

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    Logger.Write("Failed to retrieve executing file name.");
                    return;
                }

                var embeddedData = await _embeddedDataReader.TryGetEmbeddedData(filePath);

                if (embeddedData is null || embeddedData == EmbeddedServerData.Empty)
                {
                    Logger.Write("Embedded server data is empty.  Aborting.");
                    return;
                }

                if (embeddedData.ServerUrl is null)
                {
                    Logger.Write("ServerUrl is empty.  Aborting.");
                    return;
                }

                /*OrganizationID = embeddedData.OrganizationId;
                ServerUrl = embeddedData.ServerUrl.AbsoluteUri;

                using (var httpClient = new HttpClient())
                {
                    var serializer = new JavaScriptSerializer();
                    var brandingUrl = $"{ServerUrl.TrimEnd('/')}/api/branding/{OrganizationID}";
                    using (var response = await httpClient.GetAsync(brandingUrl).ConfigureAwait(false))
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        _brandingInfo = serializer.Deserialize<BrandingInfo>(responseString);

                    }
                }*/
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            finally
            {
                //ApplyBranding(_brandingInfo);
                ApplyUI();
            }
        }

        private void ApplyUI()
        {
            Stream imageStream;
            imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Remotely.Agent.Installer.Win.Assets.Tuso-Logo.png");

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = imageStream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            imageStream.Close();

            Icon = bitmap;
        }

        private BitmapImage GetBitmapImageIcon(BrandingInfo bi)
        {
            Stream imageStream;
            if (!string.IsNullOrWhiteSpace(bi?.Icon))
            {
                imageStream = new MemoryStream(Convert.FromBase64String(bi.Icon));
            }
            else
            {
                imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Remotely.Agent.Installer.Win.Assets.Tuso-Logo.png");
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = imageStream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            imageStream.Close();

            return bitmap;
        }

        public static string GetMotherBoardID()
        {
            string mbInfo = String.Empty;

            //Get motherboard's serial number 
            ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_BaseBoard");
            foreach (ManagementObject mo in mbs.Get())
            {
                mbInfo += mo["SerialNumber"].ToString();
            }
            return mbInfo;
        }

        public string GetPrivateIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public string GetPublicIPAddress()
        {
            String address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return address;
        }

        private async Task Install(string userName)
        {
            try
            {
                IsReadyState = false;
                if (!CheckParams())
                {
                    return;
                }

                HeaderMessage = "Installing Remotely...";
                string deviceID = null;
                if (((deviceID = await _installer.Install(ServerUrl, OrganizationID, DeviceGroup, DeviceAlias, DeviceUuid, CreateSupportShortcut)) != null))
                {
                    IsServiceInstalled = true;
                    Progress = 0;
                    HeaderMessage = "Installation completed.";
                    
                    if (userName != null) {

                        string privateIP = GetPrivateIPAddress();

                        var macAddress = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                                          where nic.OperationalStatus == OperationalStatus.Up
                                          select nic.GetPhysicalAddress().ToString()).FirstOrDefault();

                        var motherBoardSerial = GetMotherBoardID();

                        var publicIP = GetPublicIPAddress();

                        await updateDeviceID(userName, deviceID, privateIP, macAddress, motherBoardSerial, publicIP);
                    }
                    
                    //StatusMessage = "Remotely has been installed.  You can now close this window.";
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ShowNotification();
                        App.Current.Shutdown();
                    }));
                    return;
                }
                else
                {
                    Progress = 0;
                    HeaderMessage = "An error occurred during installation.";
                    StatusMessage = "Your installation failed. Please try again";
                    //MessageBoxEx.Show("There was an error during installation.  Check the logs for details.", "Install error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (!CheckIsAdministrator())
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            finally
            {
                IsReadyState = true;
            }
        }

        private void ShowNotification()
        {
            var notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Remotely.Agent.Installer.Win.Assets.Tuso-Icon.ico"));
            notifyIcon.Text = System.Windows.Application.Current.MainWindow.Title;
            notifyIcon.Visible = true;

            notifyIcon.ShowBalloonTip(3000, "Tuso", "Agent installed successfully", ToolTipIcon.Info);
        }

        private async Task updateDeviceID(string userName, string deviceId, string privateIP, string macAddress, string motherBoardSerial, string publicIP)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {

                    var getUser = "https://staging-tuso.api.arcapps.org/tuso-api/rdp-device-info/key/" + userName;
                    using (var response = await httpClient.GetAsync(getUser).ConfigureAwait(false))
                    {
                        var bodyData = new
                        {
                            userName = UserName,
                            deviceID = deviceId,
                            privateIP = privateIP,
                            macAddress = macAddress,
                            motherBoardSerial = motherBoardSerial,
                            publicIP = publicIP
                        };
                        var bodyJson = JsonSerializer.Serialize(bodyData);
                        var bodyContent = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json");

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var putURL = "https://staging-tuso.api.arcapps.org/tuso-api/rdp-device-info-byusername/" + userName;
                            using (var putResponse = await httpClient.PutAsync(putURL, bodyContent).ConfigureAwait(false))
                            {
                              //  return;
                                if (putResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    return;
                                }
                                else
                                {
                                    Logger.Write("Update username failed");
                                }
                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            // No user
                            var postURL = "https://staging-tuso.api.arcapps.org/tuso-api/rdp-device-info";
                            using (var postResponse = await httpClient.PostAsync(postURL, bodyContent).ConfigureAwait(false))
                            {
                                if (postResponse.StatusCode == HttpStatusCode.Created)
                                {
                                    return;
                                }
                                else
                                {
                                    Logger.Write("Register username failed");
                                }
                            }
                        } else 
                        {
                            Logger.Write("Get username failed");
                        }
                    }

                    /*var bodyJson = JsonSerializer.Serialize(bodyData);

                    

                    using (var response = await httpClient.PostAsync(loginUrl, bodyContent).ConfigureAwait(false))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var responseString = await response.Content.ReadAsStringAsync();
                            LoginResult _loginResult = JsonSerializer.Deserialize<LoginResult>(responseString);
                            ServerUrl = _loginResult.serverURL;
                            OrganizationID = _loginResult.organizationID;
                            StatusMessage = "Congratulations! You're now logged in and the app is installing.";
                            await Install(UserName);
                        }
                        else
                        {
                            StatusMessage = "Incorrect Username or Password. Please try again";
                        }
                    }*/
                }
            }
            catch (Exception e)
            {
                Logger.Write(e);
            }
            return;
        }

        private async Task SignIn()
        {
            if (!CheckParams())
            {
                return;
            }

            try
            {
                using (var httpClient = new HttpClient())
                {

                    var bodyData = new
                    {
                        userName = UserName,
                        password = Password,
                    };

                    var loginUrl = $"{RDP_SERVER}login";

                    var bodyJson = JsonSerializer.Serialize(bodyData);

                    var bodyContent = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json");

                    using (var response = await httpClient.PostAsync(loginUrl, bodyContent).ConfigureAwait(false))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {

                            var responseString = await response.Content.ReadAsStringAsync();
                            LoginResult _loginResult = JsonSerializer.Deserialize<LoginResult>(responseString);
                            ServerUrl = _loginResult.serverURL;
                            OrganizationID = _loginResult.organizationID;
                            StatusMessage = "Congratulations! You're now logged in and the app is installing.";
                            await Install(UserName);
                        }
                        else
                        {
                            StatusMessage = "Incorrect Username or Password. Please try again";
                        }
                    }
                }
            } catch (Exception e)
            {
                StatusMessage = "Incorrect Username or Password. Please try again";
            }
        }

        private async Task Uninstall()
        {
            try
            {
                IsReadyState = false;

                HeaderMessage = "Uninstalling Remotely...";

                if (await _installer.Uninstall())
                {
                    IsServiceInstalled = false;
                    Progress = 0;
                    HeaderMessage = "Uninstall completed.";
                    //StatusMessage = "Remotely has been uninstalled.  You can now close this window.";
                }
                else
                {
                    Progress = 0;
                    HeaderMessage = "An error occurred during uninstall.";
                    //StatusMessage = "There was an error during uninstall.  Check the logs for details.";
                }

            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            finally
            {
                IsReadyState = true;
            }
        }

        private void UserPassword_Changed(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Password changed");
        }
    }
}
