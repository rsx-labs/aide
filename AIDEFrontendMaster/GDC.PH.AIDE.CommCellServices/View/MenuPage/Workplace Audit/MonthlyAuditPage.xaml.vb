﻿Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports UI_AIDE_CommCellServices.ServiceReference1
Imports System.ServiceModel
Imports NLog
<CallbackBehavior(ConcurrencyMode:=ConcurrencyMode.Single, UseSynchronizationContext:=False)>
Class MonthlyAuditPage
    Implements ServiceReference1.IAideServiceCallback
    Private dailyVMM As New dayVM
    Private email As String
    Private pageframe As Frame
    Private profile As Profile
    Private addframe As Frame
    Private menugrid As Grid
    Private submenuframe As Frame
    'Private _AideService As ServiceReference1.AideServiceClient
    Dim lstAuditSchedMonth As WorkplaceAudit()
    Dim AuditSchedMonthVM As New SelectionListViewModel
    Dim workPlaceAuditVM As New WorkplaceAuditViewModel
    Dim lstEmployee As WorkplaceAudit()
    Dim lstAuditQuestions As WorkplaceAudit()
    Private currDailyAuditAssigned As Integer
    Private defaultDisplay As String
    Private defaultFy_Week As Integer
    Private LstAuditDailySchedByWeek As New ObservableCollection(Of WorkplaceAuditModel)

    Dim lstFiscalYear As FiscalYear()
    Dim commendationVM As New CommendationViewModel()
    Dim fiscalyearVM As New SelectionListViewModel
    Dim month As Integer
    Dim year As Integer
    Dim isInitialize As Boolean

    Private _logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger()

    Public Sub New(_pageframe As Frame, _profile As Profile, _addframe As Frame,
                   _menugrid As Grid, _submenuframe As Frame)



        _logger.Debug("Start : Constructor")
        Me.pageframe = _pageframe
        Me.profile = _profile
        Me.addframe = _addframe
        Me.menugrid = _menugrid
        Me.submenuframe = _submenuframe
        InitializeComponent()

        '_AideService = aideService

        'InitializeService()
        ' Add any initialization after the InitializeComponent() call.
        isInitialize = True
        LoadSChed()
        SetFiscalYear()

        If cbYear.SelectedValue = Nothing Then
            cbYear.SelectedValue = defaultDisplay
        End If
        isInitialize = False

        _logger.Debug("End : Constructor")

    End Sub
    Public Sub SetFiscalYear()
        Try
            month = Date.Now.Month

            If Today.DayOfYear() <= CDate(Today.Year().ToString + "-03-31").DayOfYear Then
                cbYear.Text = (Date.Now.Year - 1).ToString() + "-" + (Date.Now.Year).ToString()
            Else
                cbYear.Text = (Date.Now.Year).ToString() + "-" + (Date.Now.Year + 1).ToString()
            End If

            year = CInt(cbYear.SelectedValue.ToString().Substring(0, 4))


        Catch ex As Exception
            _logger.Error($"Error at SetFiscalYear. {ex.ToString()}")

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try
    End Sub

    Private Sub GenerateQuestions()
        Dim questModel As New QuestionsDayModel
        Dim imgdtcheck As String

        Try

            'If InitializeService() Then
            lstAuditQuestions = CommonUtility.Instance().AuditQuestions(2) 'AideClient.GetClient().GetAuditQuestions(profile.Emp_ID, "3")
            'End If

            Dim FYDBProvider As New WorkplaceAuditDBProvider
            LstAuditDailySchedByWeek.Clear()
            FYDBProvider.GetMyWorkplaceAudit.Clear()
            For Each objFiscal As WorkplaceAudit In lstAuditQuestions
                FYDBProvider.SetMyWorkplaceAudit(objFiscal)
            Next

            For Each rawUser As MyWorkplaceAudit In FYDBProvider.GetMyWorkplaceAudit()
                LstAuditDailySchedByWeek.Add(New WorkplaceAuditModel(rawUser))
            Next
            workPlaceAuditVM.WorkPlaceAuditLst = LstAuditDailySchedByWeek
            If month = 1 Then
                year = year + 1
            ElseIf month = 2 Then
                year = year + 1
            ElseIf month = 2 Then
                year = year + 1
            End If
            For Each quest As WorkplaceAuditModel In LstAuditDailySchedByWeek.ToList
                '0 - Not yet Check the Question
                '1 - Checked Already
                '2 - checked but not completed/success
                If month.ToString() = Date.Parse(quest.WEEKDATE).Month.ToString() AndAlso year = CDate(quest.WEEKDATE).Year Then
                    If quest.DT_CHECK_FLG = 0 Then
                        imgdtcheck = "..\..\..\Assets\Button\audittocheck.png"
                    ElseIf quest.DT_CHECK_FLG = 1 Then
                        imgdtcheck = "..\..\..\Assets\Button\Checked.png"
                    Else
                        imgdtcheck = "..\..\..\Assets\Button\wrong.png"
                    End If
                    dailyVMM.QuestionDayList.Add(New QuestionsDayModel(quest.AUDIT_QUESTIONS, quest.OWNER, quest.DT_CHECK_FLG, imgdtcheck, quest.WEEKDATE))
                End If
            Next

            QuarterLVQuestions.ItemsSource = dailyVMM.QuestionDayList
            DataContext = dailyVMM.QuestionDayList
        Catch ex As Exception
            _logger.Error($"Error at GenerateQuestion. {ex.ToString()}")

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

    End Sub

    Private Sub InitEmpAuditDailybyWeekData()
        Try
            Dim statusAudit As String = ""
            Dim LstAuditDailySchedByWeek As New ObservableCollection(Of WorkplaceAuditModel)
            Dim FYDBProvider As New WorkplaceAuditDBProvider
            Dim monthNames As String = ""

            LstAuditDailySchedByWeek.Clear()
            FYDBProvider.GetMyWorkplaceAudit.Clear()

            If cbYear.SelectedValue = Nothing Then
                cbYear.SelectedValue = String.Empty
            End If

            If Not cbYear.SelectedValue Is Nothing Then
                'Dim DateString As String = stringMonth.Substring(0, 13)
                defaultDisplay = cbYear.SelectedValue.ToString().Substring(5, 4)
            End If

            'since year today is base - let's place example day is 1
            'doesn't matter what days just get current month with day
            Dim yr As Integer
            yr = CInt(defaultDisplay)

            'If InitializeService() Then
            lstEmployee = AideClient.GetClient().GetMonthlyAuditor(profile.Emp_ID, yr)
            'End If

            If lstEmployee.Count = 0 Then
                If Not isInitialize Then
                    MsgBox("There is no records in selected fiscal year.", vbOKOnly + MsgBoxStyle.Exclamation, "AIDE")
                End If
                Return
            End If

            For Each objFiscal As WorkplaceAudit In lstEmployee
                FYDBProvider.SetMyWorkplaceAudit(objFiscal)
            Next

            For Each rawUser As MyWorkplaceAudit In FYDBProvider.GetMyWorkplaceAudit()
                LstAuditDailySchedByWeek.Add(New WorkplaceAuditModel(rawUser))
                currDailyAuditAssigned = rawUser.EMP_ID
                defaultFy_Week = rawUser.FY_WEEK

            Next
            workPlaceAuditVM.WorkPlaceAuditLst = LstAuditDailySchedByWeek
            For Each quest As WorkplaceAuditModel In LstAuditDailySchedByWeek.ToList
                Select Case quest.DT_CHECK_FLG
                    Case 0
                        statusAudit = "Not Completed"
                    Case 1
                        statusAudit = "Completed"
                    Case 2
                        statusAudit = "Completed"
                End Select
                monthNames = Date.Parse(quest.WEEKDATE).ToString("MMM")
                dailyVMM.Days.Add(New DayMod(monthNames, quest.WEEKDATE, quest.NICKNAME, statusAudit, Date.Parse(quest.WEEKDATE).Year.ToString(), quest.DATE_CHECKED, quest.WEEKDATESCHED))
            Next

            QuarterLV.ItemsSource = dailyVMM.Days
            DataContext = dailyVMM.Days

        Catch ex As Exception

            _logger.Error($"Error at InitEmpAudit. {ex.ToString()}")
            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try
    End Sub
    'Public Function InitializeService() As Boolean
    '    _logger.Debug("Start : InitializeService")

    '    Dim bInitialize As Boolean = False
    '    Try

    '        If _AideService.State = CommunicationState.Faulted Then

    '            _logger.Debug("Service is faulted, reinitializing ...")

    '            Dim Context As InstanceContext = New InstanceContext(Me)
    '            _AideService = New AideServiceClient(Context)
    '            _AideService.Open()
    '        End If

    '        bInitialize = True
    '    Catch ex As SystemException
    '        _logger.Error(ex.ToString())

    '        _AideService.Abort()
    '        MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
    '    End Try

    '    _logger.Debug("End : InitializeService")

    '    Return bInitialize
    'End Function
    Public Sub LoadYear()
        Try
            'If InitializeService() Then
            lstFiscalYear = CommonUtility.Instance().FiscalYears 'AideClient.GetClient().GetAllFiscalYear()
            LoadFiscalYear()
            'End If
        Catch ex As Exception
            _logger.Error($"Error at LoadYear. {ex.ToString()}")

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")

        End Try
    End Sub
    Public Sub LoadFiscalYear()
        Try
            Dim lstFiscalYearList As New ObservableCollection(Of FiscalYearModel)
            Dim FYDBProvider As New SelectionListDBProvider

            For Each objFiscal As FiscalYear In lstFiscalYear
                FYDBProvider._setlistofFiscal(objFiscal)
            Next

            For Each rawUser As myFiscalYearSet In FYDBProvider._getobjFiscal()
                lstFiscalYearList.Add(New FiscalYearModel(rawUser))
            Next

            fiscalyearVM.ObjectFiscalYearSet = lstFiscalYearList
            cbYear.ItemsSource = fiscalyearVM.ObjectFiscalYearSet
        Catch ex As Exception
            _logger.Error($"Error at LoadFiscalYear. {ex.ToString()}")

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try
    End Sub
    Public Sub LoadSChed()
        Try
            'If InitializeService() Then
            lstAuditSchedMonth = AideClient.GetClient().GetAuditSChed_Month(2, Date.Now.Year, Date.Now.Month)
            LoadYear()
            'End If
        Catch ex As Exception
            _logger.Error($"Error at LoadSched. {ex.ToString()}")

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try
    End Sub



    Private Sub ListViewItem_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If profile.Emp_ID = currDailyAuditAssigned OrElse profile.Permission_ID = 1 Then
            Dim item = (TryCast(sender, FrameworkElement)).DataContext
            Dim FYDBProvider As New WorkplaceAuditDBProvider

            For Each objFiscal As WorkplaceAudit In lstAuditQuestions
                If objFiscal.WEEKDATE.ToString.Trim() = item.WEEKDATE.ToString.Trim() Then
                    FYDBProvider.SetMyWorkplaceAudit(objFiscal)
                End If

            Next
            LstAuditDailySchedByWeek.Clear()
            For Each rawUser As MyWorkplaceAudit In FYDBProvider.GetMyWorkplaceAudit()
                LstAuditDailySchedByWeek.Add(New WorkplaceAuditModel(rawUser))
            Next

            addframe.Navigate(New DailyAuditCheck(pageframe, profile, addframe, menugrid, submenuframe, LstAuditDailySchedByWeek, 3))
            pageframe.IsEnabled = False
            pageframe.Opacity = 0.3
            menugrid.IsEnabled = False
            menugrid.Opacity = 0.3
            submenuframe.IsEnabled = False
            submenuframe.Opacity = 0.3
            addframe.Visibility = Visibility.Visible
            addframe.Margin = New Thickness(120, 60, 120, 60)
        End If

    End Sub


    Public Sub NotifySuccess(message As String) Implements IAideServiceCallback.NotifySuccess
        Throw New NotImplementedException()
    End Sub

    Public Sub NotifyError(message As String) Implements IAideServiceCallback.NotifyError
        Throw New NotImplementedException()
    End Sub

    Public Sub NotifyPresent(EmployeeName As String) Implements IAideServiceCallback.NotifyPresent
        Throw New NotImplementedException()
    End Sub

    Public Sub NotifyOffline(EmployeeName As String) Implements IAideServiceCallback.NotifyOffline
        Throw New NotImplementedException()
    End Sub

    Public Sub NotifyUpdate(objData As Object) Implements IAideServiceCallback.NotifyUpdate
        Throw New NotImplementedException()
    End Sub

    Private Sub SetSelectedDay(sender As Object, e As SelectionChangedEventArgs)
        Dim item As Object
        Dim questModel As New QuestionsDayModel
        Dim imgdtcheck As String
        If Not QuarterLV.SelectedItem Is Nothing Then
            item = QuarterLV.SelectedItem.WEEK_DATE_SCHED
        Else

            Return
        End If

        dailyVMM.QuestionDayList.Clear()

        Try
            For Each quest As WorkplaceAuditModel In LstAuditDailySchedByWeek.ToList
                '0 - Not yet Check the Question
                '1 - Checked Already
                '2 - checked but not completed/success

                If item.ToString.Trim() = quest.WEEKDATE.ToString.Trim() AndAlso CDate(item).Year = CDate(quest.WEEKDATE).Year Then
                    If quest.DT_CHECK_FLG = 0 Then
                        imgdtcheck = "..\..\..\Assets\Button\audittocheck.png"
                    ElseIf quest.DT_CHECK_FLG = 1 Then
                        imgdtcheck = "..\..\..\Assets\Button\Checked.png"
                    Else
                        imgdtcheck = "..\..\..\Assets\Button\wrong.png"
                    End If
                    currDailyAuditAssigned = quest.EMP_ID
                    dailyVMM.QuestionDayList.Add(New QuestionsDayModel(quest.AUDIT_QUESTIONS, quest.OWNER, quest.DT_CHECK_FLG, imgdtcheck, quest.WEEKDATE))
                End If
            Next

            QuarterLVQuestions.ItemsSource = dailyVMM.QuestionDayList
            DataContext = dailyVMM.QuestionDayList
        Catch ex As Exception
            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

    End Sub

    Private Sub cbYear_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbYear.SelectionChanged
        If Not cbYear.SelectedIndex = -1 Then
            year = CInt(cbYear.SelectedValue.ToString().Substring(0, 4))
            dailyVMM.QuestionDayList.Clear()
            dailyVMM.Days.Clear()
            InitEmpAuditDailybyWeekData()
            If dailyVMM.Days.Count <> 0 Then
                GenerateQuestions()
            End If
        End If



    End Sub
End Class



