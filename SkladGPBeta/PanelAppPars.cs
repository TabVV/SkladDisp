using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;

using PDA.OS;
using PDA.Service;


namespace SkladGP
{
    public partial class MainF : Form
    {

        //private Dictionary<int, string> dicMTypes;

        // ������� �� ������� ������
        private void EnterInPars()
        {
            if (xSm.urCur > Smena.USERRIGHTS.USER_KLAD)
            {
                tcPars.Enabled = true;
                if (cmbField.SelectedIndex < 0)
                    cmbField.SelectedIndex = 0;
                if (cmbMType.SelectedIndex < 0)
                    cmbMType.SelectedIndex = 0;
                if (cmbDocType.SelectedIndex < 0)
                    cmbDocType.SelectedIndex = 0;
                SetEditMode(true);
                tSrvParServer.Focus();
            }
        }

        // ��������� ������� � ������ �� ������
        private bool AppPars_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool 
                bNextOrPrev,
                ret = false;
            int 
                nR;
            Control 
                xC;

            if (nFunc > 0)
            {
                switch (nFunc)
                {
                    case AppC.F_UPLD_DOC:               // ���������� ����������
                        nR = AppPars.SavePars(xPars);
                        if (AppC.RC_OK == nR)
                        {
                            Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                            MessageBox.Show("��������� ���������", "����������");
                        }
                        else
                        {
                            Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                            MessageBox.Show("������ ����������!", "����������");
                        }
                        ret = true;
                        break;
                    case AppC.F_CHGSCR:
                        break;
                }
            }
            else
            {// ��� ������ �������
                switch (e.KeyValue)
                {
                    case W32.VK_ESC:
                        // ����� ������� � 
                        xC = Srv.GetPageControl(tpParPaths, 1);
                        // ������� �� ����������
                        xC.Parent.SelectNextControl(xC, false, true, false, true);
                        Back2Main();
                        ret = true;
                        break;
                    case W32.VK_ENTER:
                        // ����� ������� � 
                        xC = Srv.GetPageControl(tpParPaths, 1);
                        // ������� �� ���������
                        bNextOrPrev = (e.Modifiers == Keys.None) ? true : false;
                        xC.Parent.SelectNextControl(xC, bNextOrPrev, true, false, true);
                        ret = true;
                        break;
                    case W32.VK_F2:               // ���������� ���������� � ���������� PC
                        nR = AppPars.SavePars(xPars);
                        if (AppC.RC_OK == nR)
                        {
                            Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                            MessageBox.Show("��������� ���������", "����������");
                        }
                        else
                        {
                            Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                            MessageBox.Show("������ ����������!", "����������");
                        }
                        ret = true;
                        break;
                }
            }
            e.Handled |= ret;
            return (ret);
        }




        private void SetParAppFields()
        {
            DocTypeInf
                xD;

            cbShiftOnly.Checked = AppPars.bArrowsWithShift;
            CurOper.xMF = this;

            //DocPars.dicTypD = new Dictionary<int, string>();
            //DocPars.dicTypD.Add(AppC.TYPD_SAM, "���������");
            //DocPars.dicTypD.Add(AppC.TYPD_CVYV, "�����������");
            //DocPars.dicTypD.Add(AppC.TYPD_SVOD, "����");
            //DocPars.dicTypD.Add(AppC.TYPD_VPER, "�_�����������");
            //DocPars.dicTypD.Add(AppC.TYPD_SCHT, "����");
            //DocPars.dicTypD.Add(AppC.TYPD_INV, "��������������");
            //DocPars.dicTypD.Add(AppC.TYPD_OPR, "<-��������->");
            //DocPars.dicTypD.Add(AppC.TYPD_BRK, "����");
            //DocPars.dicTypD.Add(AppC.TYPD_PRIH, "���������");
            //DocPars.dicTypD.Add(AppC.TYPD_ZKZ, "�����");

            AppC.xDocTInf = new Dictionary<int, DocTypeInf>();
            xD = new DocTypeInf(AppC.TYPD_SAM, "���������", true, false, AppC.MOVTYPE.RASHOD); //
            AppC.xDocTInf.Add(AppC.TYPD_SAM, xD);

            xD = new DocTypeInf(AppC.TYPD_CVYV, "�����������", true, false, AppC.MOVTYPE.RASHOD); //
            AppC.xDocTInf.Add(AppC.TYPD_CVYV, xD);

            xD = new DocTypeInf(AppC.TYPD_SVOD, "����", true, false, AppC.MOVTYPE.RASHOD); //
            AppC.xDocTInf.Add(AppC.TYPD_SVOD, xD);

            xD = new DocTypeInf(AppC.TYPD_VPER, "�_�����������", true, false, AppC.MOVTYPE.RASHOD); //
            AppC.xDocTInf.Add(AppC.TYPD_VPER, xD);

            xD = new DocTypeInf(AppC.TYPD_SCHT, "����", true, false, AppC.MOVTYPE.RASHOD); //
            AppC.xDocTInf.Add(AppC.TYPD_SCHT, xD);

            xD = new DocTypeInf(AppC.TYPD_INV, "��������������", true, false, AppC.MOVTYPE.AVAIL); //
            //xD.TryFrom = false;
            xD.TryFrom = true;
            AppC.xDocTInf.Add(AppC.TYPD_INV, xD);

            xD = new DocTypeInf(AppC.TYPD_OPR, "���������������", true, true, AppC.MOVTYPE.MOVEMENT); //
            AppC.xDocTInf.Add(AppC.TYPD_OPR, xD);

            xD = new DocTypeInf(AppC.TYPD_BRK, "����", true, false, AppC.MOVTYPE.AVAIL); //
            AppC.xDocTInf.Add(AppC.TYPD_BRK, xD);

            xD = new DocTypeInf(AppC.TYPD_PRIH, "���������", false, true, AppC.MOVTYPE.PRIHOD); //
            AppC.xDocTInf.Add(AppC.TYPD_PRIH, xD);

            xD = new DocTypeInf(AppC.TYPD_ZKZ, "�����", true, false, AppC.MOVTYPE.RASHOD); //
            AppC.xDocTInf.Add(AppC.TYPD_ZKZ, xD);

        }

        private void cbConfVes_Validating(object sender, CancelEventArgs e)
        {
            AppPars.bVesNeedConfirm = chbConfMest.Checked;
        }

        // ������� ������ � Shift
        private void cbShiftOnly_Validating(object sender, CancelEventArgs e)
        {
            AppPars.bArrowsWithShift = cbShiftOnly.Checked;
        }

        // ����� �������� ���������� ��� ����������� � ����
        //private void cbDocCtrl_CheckStateChanged(object sender, EventArgs e)
        //{
        //    xPars.parDocControl = cbDocCtrl.Checked;
        //    if (xPars.parDocControl == true)
        //        tDocCtrlState.Text = "�";
        //    else
        //        tDocCtrlState.Text = "";
        //}



        private void cmbField_SelectedIndexChanged(object sender, EventArgs e)
        {
            //xPars.CurField = ((ComboBox)sender).SelectedIndex;
            int nI = ((ComboBox)sender).SelectedIndex;
            if (nI >= 0)
            {
                xPars.CurField = nI;
                cbAfterScan.DataBindings[0].ReadValue();
                cbAvEdit.DataBindings[0].ReadValue();
                cbAvVvod.DataBindings[0].ReadValue();
            }
        }

        // �������� ��� ���������
        private void cmbMType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nI = ((ComboBox)sender).SelectedIndex;
            if (nI >= 0)
            {
                xPars.CurVesType = nI;
                chbConfMest.DataBindings[0].ReadValue();
                chbChkMaxPoddon.DataBindings[0].ReadValue();
                tVesVar.DataBindings[0].ReadValue();
                chbStartQ.DataBindings[0].ReadValue();

                cbAfterScan.DataBindings[0].ReadValue();
                cbAvEdit.DataBindings[0].ReadValue();
                cbAvVvod.DataBindings[0].ReadValue();
            }
        }

        // �������� ��� ���������
        private void cmbDocType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //xPars.CurDocType = ((ComboBox)sender).SelectedIndex;
            int nI = ((ComboBox)sender).SelectedIndex;
            if (nI >= 0)
            {
                xPars.CurDocType = nI;
                cbKolFromZ.DataBindings[0].ReadValue();
                cbDocCtrl.DataBindings[0].ReadValue();
                cbSumVes.DataBindings[0].ReadValue();
                cbConfScan.DataBindings[0].ReadValue();
            }
        }

        // �������� ���� ������� �� ����������� ���������
        private void cbHidUpl_CheckStateChanged(object sender, EventArgs e)
        {
            if (xNSI != null)
                FiltForDocs(((CheckBox)sender).Checked, xNSI.DT[NSI.BD_DOCOUT]);
        }

        private void SetBindAppPars()
        {
            Binding bi;

            bi = new Binding("Text", xPars, "sAppStore");
            tSrvAppPath.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "sNSIPath");
            tNsiPath.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "sDataPath");
            tDataPath.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "sHostSrv");
            tSrvParServer.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "nSrvPort");
            tSrvParServPort.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "nSrvPortM");
            tSrvParServPortM.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "NTPSrv");
            tNTPSrv.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "bWaitSock");
            cbWaitSock.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "bAutoSave");
            cbAutoSave.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "bUseSrvG");
            chUseSrvG.DataBindings.Add(bi);

            // ��������� ����� ��� ����� ���������
            bi = new Binding("Checked", xPars, "bConfMest");
            chbConfMest.DataBindings.Add(bi);
            bi = new Binding("Checked", xPars, "bMaxKolEQPodd");
            chbChkMaxPoddon.DataBindings.Add(bi);
            bi = new Binding("Text", xPars, "MaxVesVar");
            tVesVar.DataBindings.Add(bi);
            bi = new Binding("Checked", xPars, "bStart1stPoddon");
            chbStartQ.DataBindings.Add(bi);

            // �� �����
            bi = new Binding("Checked", xPars, "bAfterScan");
            cbAfterScan.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "bEdit");
            cbAvEdit.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "bManual");
            cbAvVvod.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "WarnNewScan");
            cbWarnNewScan.DataBindings.Add(bi);

            // ��� ����� ���������
            // ���������� ���������� �� ������
            bi = new Binding("Checked", xPars, "bKolFromZvk");
            cbKolFromZ.DataBindings.Add(bi);

            // �������� ����� ���������
            bi = new Binding("Checked", xPars, "bTestBeforeUpload");
            cbDocCtrl.DataBindings.Add(bi);

            // ����������� ������� ���������
            bi = new Binding("Checked", xPars, "bSumVesProd");
            cbSumVes.DataBindings.Add(bi);


            bi = new Binding("Text", xPars, "Days2Save");
            tDays2Save.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "ReLogon");
            tReLogon.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "bHideUploaded");
            cbHidUpl.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "OpAutoUpl");
            cbAutoUpLoadOper.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "OpChkAdr");
            cbChkOpr.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "UseFixAddr");
            cbUseFAddr.DataBindings.Add(bi);

            // ����������� ������ ����� ������������
            bi = new Binding("Checked", xPars, "ConfScan");
            cbConfScan.DataBindings.Add(bi);

            // ����������� ������ ����� ������������
            bi = new Binding("Checked", xPars, "CanEditIDNum");
            cbIDByHand.DataBindings.Add(bi);

            // �������� ��������� ������ �� ������ ��� ���������������
            bi = new Binding("Checked", xPars, "UseAdr4DocMode");
            cbUseAdr4Doc.DataBindings.Add(bi);


            // ������ ��������
            //bi = new Binding("DataSource", xPars, "bHideUploaded");
            //cmbHostG.DataBindings.Add(bi);


        }


    }
}
