using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace Sample
{
    class Helper
    {
        static string strConnectedMFPIP = string.Empty;
        public static string MFPIP
        {
            get => strConnectedMFPIP;
            set => strConnectedMFPIP = value;
        }

        private static ResourceLoader loader = new ResourceLoader();

        public static string GetResourceString(string strID)
        {
            string strValue = string.Empty;
            if (loader != null)
            {
                strValue = loader.GetString(strID);
            }
            return strValue;
        }

        public static ContentDialog GetDialog()
        {
            ContentDialog chkDialog = new ContentDialog();
            try
            {
                chkDialog.Title = GetResourceString("IDS_CHKDLG_TITLE");
                chkDialog.PrimaryButtonText = GetResourceString("IDS_CHKDLG_OK");
                chkDialog.CloseButtonText = GetResourceString("IDS_CHKDLG_CLOSE");
            }
            catch (Exception ex)
            {
                
            }
            return chkDialog;
        }

        public static void GetDisplayStrings(string strCode, out string strTitle, out string strContent)
        {
            strTitle = string.Empty;
            strContent = string.Empty;

            if (!string.IsNullOrEmpty(strCode))
            {
                switch (strCode)
                {
                    case "ADUMissingError":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_ADUOPEN");
                        strContent = Helper.GetResourceString("ID_NT_MSG_ADUOPEN");
                        break;

                    case "ADFCoverOpen":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_ADFOPEN");
                        strContent = Helper.GetResourceString("ID_NT_MSG_ADFOPEN");
                        break;

                    case "Drawer1Open":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_DRAW1OPEN");
                        strContent = Helper.GetResourceString("ID_NT_MSG_DRAW1OPEN");
                        break;

                    case "Drawer2Open":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_DRAW2OPEN");
                        strContent = Helper.GetResourceString("ID_NT_MSG_DRAW2OPEN");
                        break;

                    case "Drawer3Open":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_DRAW3OPEN");
                        strContent = Helper.GetResourceString("ID_NT_MSG_DRAW3OPEN");
                        break;

                    case "Drawer4Open":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_DRAW4OPEN");
                        strContent = Helper.GetResourceString("ID_NT_MSG_DRAW4OPEN");
                        break;

                    case "Drawer1PaperEmpty":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_DRAW1EMPTY");
                        strContent = Helper.GetResourceString("ID_NT_MSG_DRAW1EMPTY");
                        break;

                    case "Drawer2PaperEmpty":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_DRAW2EMPTY");
                        strContent = Helper.GetResourceString("ID_NT_MSG_DRAW2EMPTY");
                        break;

                    case "Drawer3PaperEmpty":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_DRAW3EMPTY");
                        strContent = Helper.GetResourceString("ID_NT_MSG_DRAW3EMPTY");
                        break;

                    case "Drawer4PaperEmpty":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_DRAW4EMPTY");
                        strContent = Helper.GetResourceString("ID_NT_MSG_DRAW4EMPTY");
                        break;

                    case "WasteTonerBoxNearFull":
                        strTitle = Helper.GetResourceString("ID_NT_TITLE_NEWWASTETONER");
                        strContent = Helper.GetResourceString("ID_NT_MSG_NEWWASTETONER");
                        break;
                }
            }
        }
    }
}