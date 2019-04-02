using Sample.DeviceInfoService;
using Sample.OutputManagementService;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Sample
{
    class Stage2Interface
    {
        static Stage2Interface _instance = null;
        EndpointAddress _address = null;
        string _loginToken = null;
        string _stage2Port = "49629";
        bool bIsStage2InitDone = false;

        string strMFPIP = string.Empty;
        string _strUserName = string.Empty;
        string _strPassword = string.Empty;

        const string INVALID_CRED_FAULT_TYPE = "ERR_EBS_AUTH_INVALID_CREDENTIALS";

        public static Stage2Interface GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Stage2Interface();
            }
            return _instance;
        }

        private Stage2Interface()
        {
            strMFPIP = Helper.MFPIP;
        }

        ~Stage2Interface()
        {
            strMFPIP = string.Empty;
            _address = null;
            _loginToken = null;
            bIsStage2InitDone = false;
        }

        public bool IsStag2Inited
        {
            get => bIsStage2InitDone;
        }

        public async Task InternalLogin()
        {
            try
            {
                bIsStage2InitDone = false;
                await InternalLogin(_strUserName, _strPassword);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task InternalLogin(string strUserName, string strPassword)
        {
            _strUserName = strUserName;
            _strPassword = strPassword;

            if (!bIsStage2InitDone)
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.SendTimeout = new TimeSpan(0, 0, 15);
                Uri URI = new Uri("http://" + strMFPIP + ":" + _stage2Port);
                EndpointAddress endPoint = new EndpointAddress(URI);

                OutputManagementServicePortTypeClient proxy = new OutputManagementServicePortTypeClient(binding, endPoint);
                proxy.Endpoint.Address = endPoint;

                userAuthenticationType authObj = new userAuthenticationType();
                authObj.LoginName = _strUserName;
                authObj.Password = _strPassword;

                login loginObj = new login();
                loginObj.Authentication = authObj;
                loginResponse1 responseLogin = await proxy.loginAsync(loginObj);
                sessionTokenType sesnToken = responseLogin.loginResponse.Token;
                _loginToken = sesnToken.Token;
                _address = endPoint;
                bIsStage2InitDone = true;
            }
            else
            {
                // Already Inited no just return
            }
        }

        public async Task<OutputManagementService.getBasicDeviceInfoResponse1> GetBasicDeviceInfoAsync()
        {
            OutputManagementService.getBasicDeviceInfoResponse1 devInfoResp = null;
            
            BasicHttpBinding bindObj = new BasicHttpBinding();
            bindObj.MaxReceivedMessageSize = 20000000;
            OutputManagementServicePortTypeClient proxy = new OutputManagementServicePortTypeClient(bindObj, _address);

            OutputManagementService.getBasicDeviceInfo devInfo = new OutputManagementService.getBasicDeviceInfo();
            sessionTokenType sessnToken = new sessionTokenType();
            sessnToken.Token = _loginToken;

            devInfoResp = await proxy.getBasicDeviceInfoAsync(devInfo);
            
            return devInfoResp;
        }

        public async Task<getDeviceStatusResponse1> GetDeviceStatusAsync()
        {
            getDeviceStatusResponse1 devStatusResp = null;
            
            BasicHttpBinding bindObj = new BasicHttpBinding();
            bindObj.MaxReceivedMessageSize = 20000000;
            OutputManagementServicePortTypeClient proxy = new OutputManagementServicePortTypeClient(bindObj, _address);

            getDeviceStatus devStatus = new getDeviceStatus();
            sessionTokenType sessnToken = new sessionTokenType();
            sessnToken.Token = _loginToken;

            devStatusResp = await proxy.getDeviceStatusAsync(devStatus);
            
            return devStatusResp;
        }

        public async Task<getHardwareInfoResponse1> GetHardwareInfoAsync()
        {
            getHardwareInfoResponse1 hwResp = null;
            
            BasicHttpBinding bindObj = new BasicHttpBinding();
            bindObj.MaxReceivedMessageSize = 20000000;
            DeviceInfoServicePortTypeClient proxy = new DeviceInfoServicePortTypeClient(bindObj, _address);

            getHardwareInfo hwInfo = new getHardwareInfo();
            sessionTokenType sessnToken = new sessionTokenType();
            sessnToken.Token = _loginToken;

            hwResp = await proxy.getHardwareInfoAsync(hwInfo);
            
            return hwResp;
        }

        public async Task<getDeviceTotalResponse1> GetDeviceTotalAsync()
        {
            getDeviceTotalResponse1 devTotalResp = null;
            
            BasicHttpBinding bindObj = new BasicHttpBinding();
            bindObj.MaxReceivedMessageSize = 20000000;
            OutputManagementServicePortTypeClient proxy = new OutputManagementServicePortTypeClient(bindObj, _address);

            getDeviceTotal devTotal = new getDeviceTotal();
            devTotal.CounterName = deviceTotalCounterNameType.DeviceTotal;
            sessionTokenType sessnToken = new sessionTokenType();
            sessnToken.Token = _loginToken;

            devTotalResp = await proxy.getDeviceTotalAsync(devTotal);
            
            return devTotalResp;
        }

        public async Task<getAllWorkflowResponse1> GetAllWorkFlows()
        {
            getAllWorkflowResponse1 workflowResponse = null;
            
            BasicHttpBinding bindObj = new BasicHttpBinding();
            bindObj.SendTimeout = new TimeSpan(0, 0, 30);
            bindObj.MaxReceivedMessageSize = 20000000;
            OutputManagementServicePortTypeClient proxy = new OutputManagementServicePortTypeClient(bindObj, _address);

            getAllWorkflow allWorkflow = new getAllWorkflow();
            sessionTokenType sessnToken = new sessionTokenType();
            sessnToken.Token = _loginToken;

            workflowResponse = await proxy.getAllWorkflowAsync(allWorkflow);
            
            return workflowResponse;
        }

        public async Task Logout()
        {
            try
            {
                if (bIsStage2InitDone)
                {
                    BasicHttpBinding binding = new BasicHttpBinding();
                    binding.SendTimeout = new TimeSpan(0, 0, 15);
                    Uri URI = new Uri("http://" + strMFPIP + ":" + _stage2Port);
                    EndpointAddress endPoint = new EndpointAddress(URI);

                    OutputManagementServicePortTypeClient proxy = new OutputManagementServicePortTypeClient(binding, endPoint);
                    proxy.Endpoint.Address = endPoint;

                    logout logoutObj = new logout();
                    logoutResponse1 logoutResponse = await proxy.logoutAsync(logoutObj);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}