﻿Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports UI_AIDE_CommCellServices.ServiceReference1
Imports System.ServiceModel
Imports NLog
<CallbackBehavior(ConcurrencyMode:=ConcurrencyMode.Single, UseSynchronizationContext:=False)>
Class WeeklyAuditPage
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
    Private Month As Integer
    Dim year As Integer
    Dim isInitialize As Boolean

    Dim lstFiscalYear As FiscalYear()
    Dim fiscalyearVM As New SelectionListViewModel

    Private _logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger()

    Public Sub New(_pageframe As Frame, _profile As Profile, _addframe As Frame, _menugrid As Grid,
                   _submenuframe As Frame)

        _logger.Debug("Start : Constructor")

        Me.pageframe = _pageframe
        Me.profile = _profile
        Me.addframe = _addframe
        Me.menugrid = _menugrid
        Me.submenuframe = _submenuframe
        ' This call is required by the designer.
        InitializeComponent()

        '_AideService = aideService

        'InitializeService()
        ' Add any initialization after the InitializeComponent() call.
        isInitialize = True
        LoadYear()
        LoadSChed()
        SetFiscalYear()
        'isSetDefault = True

        If Not cbMonth.SelectedItem Is Nothing Then
            Month = Date.Now.Month
            cbMonth.SelectedValue = Month
        End If

        isInitialize = False

        _logger.Debug("End : Constructor")
    End Sub
    Public Sub LoadYear()

        _logger.Debug("Start : LoadYear")

        Try
            'If InitializeService() Then
            lstFiscalYear = CommonUtility.Instance().FiscalYears 'AideClient.GetClient().GetAllFiscalYear()
            LoadFiscalYear()
            'End If
        Catch ex As Exception
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : LoadYear")

    End Sub
    Public Sub LoadFiscalYear()

        _logger.Debug("Start : LoadFiscalYear")

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
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : LoadFiscalYear")

    End Sub

    Public Sub SetFiscalYear()

        _logger.Debug("Start : StartFiscalYear")

        Try
            Month = Date.Now.Month

            If Today.DayOfYear() <= CDate(Today.Year().ToString + "-03-31").DayOfYear Then
                cbYear.SelectedValue = (Date.Now.Year - 1).ToString() + "-" + (Date.Now.Year).ToString()
            Else
                cbYear.SelectedValue = (Date.Now.Year).ToString() + "-" + (Date.Now.Year + 1).ToString()
            End If

            year = CInt(cbYear.SelectedValue.ToString().Substring(0, 4))
            If Month = 1 Then
                year += 1
            ElseIf Month = 2 Then
                year += 1
            ElseIf Month = 3 Then
                year += 1
            End If

        Catch ex As Exception
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : StartFiscalYear")
    End Sub
    Private Sub GenerateQuestions()

        _logger.Debug("Start : GenerateQuestions")

        Dim questModel As New QuestionsDayModel
        Dim imgdtcheck As String

        Try

            'If InitializeService() Then
            lstAuditQuestions = CommonUtility.Instance().AuditQuestions(1) 'AideClient.GetClient().GetAuditQuestions(profile.Emp_ID, "2")
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

            For Each quest As WorkplaceAuditModel In LstAuditDailySchedByWeek.ToList
                '0 - Not yet Check the Question
                '1 - Checked Already
                '2 - checked but not completed/success
                Dim stringMonth As String
                Dim DateNow As String

                stringMonth = CDate(defaultDisplay).Date.Year & "-" & CDate(defaultDisplay).Date.Month.ToString("00") & "-" & CDate(defaultDisplay).Date.Day.ToString("00")
                DateNow = CDate(quest.WEEKDATE).Year & "-" & CDate(quest.WEEKDATE).Month.ToString("00") & "-" & CDate(quest.WEEKDATE).Day.ToString("00")
                If CDate(stringMonth).Year = CDate(DateNow).Year Then
                    If CDate(stringMonth).Month = CDate(DateNow).Month Then
                        If GetWeekNumber(CDate(stringMonth)) = GetWeekNumber(CDate(quest.WEEKDATE)) Then
                            If quest.DT_CHECK_FLG = 0 Then
                                imgdtcheck = "..\..\..\Assets\Button\audittocheck.png"
                            ElseIf quest.DT_CHECK_FLG = 1 Then
                                imgdtcheck = "..\..\..\Assets\Button\Checked.png"
                            Else
                                imgdtcheck = "..\..\..\Assets\Button\wrong.png"
                            End If
                            dailyVMM.QuestionDayList.Add(New QuestionsDayModel(quest.AUDIT_QUESTIONS, quest.OWNER, quest.DT_CHECK_FLG, imgdtcheck, quest.WEEKDATE))
                        End If
                    End If
                End If
            Next

            QuarterLVQuestions.ItemsSource = dailyVMM.QuestionDayList
            DataContext = dailyVMM.QuestionDayList
        Catch ex As Exception
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : GenerateQuestions")
    End Sub
    Public Shared Function GetWeekNumber(ByVal datum As Date) As Integer
        Return (datum.Day - 1) \ 7 + 1
    End Function
    Private Sub InitEmpAuditDailybyWeekData()

        _logger.Debug("Start : InitEmpAuditDailyByWeekData")

        Try
            Dim statusAudit As String = ""
            Dim LstAuditDailySchedByWeek As New ObservableCollection(Of WorkplaceAuditModel)
            Dim FYDBProvider As New WorkplaceAuditDBProvider

            LstAuditDailySchedByWeek.Clear()
            FYDBProvider.GetMyWorkplaceAudit.Clear()

            If cbMonth.SelectedValue = Nothing Then
                cbMonth.SelectedValue = String.Empty
            End If


            If Not cbMonth.SelectedItem.AUDITSCHED_MONTH Is Nothing Then
                Dim stringMonth As Integer = cbMonth.SelectedItem.FY_WEEK
                'Dim DateString As String = stringMonth.Substring(0, 13)
                defaultDisplay = stringMonth.ToString("00")
            End If

            If defaultDisplay = Date.Now.Month.ToString() Then
                defaultDisplay = Date.Now.Month.ToString("00")
            End If
            'since year today is base - let's place example day is 1
            'doesn't matter what days just get current month with day
            If Not cbYear.SelectedItem.FISCAL_year Is Nothing Then
                year = cbYear.SelectedItem.FISCAL_year.ToString.Substring(0, 4)
            End If

            If defaultDisplay = 1 Then
                year = year + 1
            ElseIf defaultDisplay = 2 Then
                year = year + 1
            ElseIf defaultDisplay = 3 Then
                year = year + 1
            Else
                year = year
            End If


            defaultDisplay = year.ToString() & "-" & defaultDisplay & "-" & "01"

            'If InitializeService() Then
            lstEmployee = AideClient.GetClient().GetWeeklyAuditor(profile.Emp_ID, Date.Parse(defaultDisplay))
            'End If


            For Each objFiscal As WorkplaceAudit In lstEmployee
                FYDBProvider.SetMyWorkplaceAudit(objFiscal)
            Next

            For Each rawUser As MyWorkplaceAudit In FYDBProvider.GetMyWorkplaceAudit()
                LstAuditDailySchedByWeek.Add(New WorkplaceAuditModel(rawUser))
                currDailyAuditAssigned = rawUser.EMP_ID


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
                dailyVMM.Days.Add(New DayMod(quest.FY_WEEK, quest.WEEKDATE, quest.NICKNAME, statusAudit, quest.WEEKDATE, quest.DATE_CHECKED, quest.WEEKDATESCHED))
            Next

            QuarterLV.ItemsSource = dailyVMM.Days
            DataContext = dailyVMM.Days

        Catch ex As Exception
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : InitEmpAudit")

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

    Public Sub LoadSChed()

        _logger.Debug("Start : LoadSched")

        Try
            'If InitializeService() Then
            lstAuditSchedMonth = AideClient.GetClient().GetAuditSChed_Month(2, Date.Now.Year, Date.Now.Month)
            LoadPerWeekSchedule()
            'End If
        Catch ex As Exception
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : LoadSched")

    End Sub

    Public Sub LoadPerWeekSchedule()

        _logger.Debug("Start : LoadPerWeekSchedule")

        Try
            Dim lstAuditSchedMonthList As New ObservableCollection(Of WorkplaceAuditModel)
            Dim FYDBProvider As New SelectionListDBProvider
            For Each objFiscal As WorkplaceAudit In lstAuditSchedMonth
                FYDBProvider._setListOfAuditSchedMonth(objFiscal)
            Next

            For Each rawUser As myAuditSchedMonthSet In FYDBProvider._getAuditSchedMonth()
                lstAuditSchedMonthList.Add(New WorkplaceAuditModel(rawUser))
                If defaultFy_Week = 0 Then
                    defaultFy_Week = rawUser._fy_week
                End If

            Next
            AuditSchedMonthVM.ObjectAuditSchedMonthSet = lstAuditSchedMonthList
            cbMonth.ItemsSource = AuditSchedMonthVM.ObjectAuditSchedMonthSet

        Catch ex As Exception
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : LoadPerWeekSchedule")

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

            addframe.Navigate(New DailyAuditCheck(pageframe, profile, addframe, menugrid, submenuframe, LstAuditDailySchedByWeek, 2))
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

        _logger.Debug("Start : SetSelectedYear")

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

                If item.ToString.Trim() = quest.WEEKDATE.ToString.Trim() Then
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
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : SetSelectedYear")

    End Sub

    Private Sub cbMonth_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbMonth.SelectionChanged

        _logger.Debug("Start : MonthSelectedChanged")

        Dim selectedFy_Week As Integer

        selectedFy_Week = defaultFy_Week
        selectedFy_Week = sender.selecteditem.fy_week
        cbMonth.SelectedValue = selectedFy_Week
        year = cbYear.SelectedItem.FISCAL_year.ToString.Substring(0, 4)

        If Month = 1 Then
            year += 1
        ElseIf Month = 2 Then
            year += 1
        ElseIf Month = 3 Then
            year += 1
        End If

        dailyVMM.QuestionDayList.Clear()
        dailyVMM.Days.Clear()
        InitEmpAuditDailybyWeekData()

        If dailyVMM.Days.Count <> 0 Then
            GenerateQuestions()
        Else
            If Not isInitialize Then
                MsgBox("There is no records in selected date.  ", vbOKOnly + MsgBoxStyle.Exclamation, "AIDE")
            End If
        End If


        _logger.Debug("End : MothSelectedChanged")
    End Sub

    Private Sub cbYear_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbYear.SelectionChanged

        _logger.Debug("Start : YearSelectionChanged")

        year = cbYear.SelectedItem.FISCAL_year.ToString.Substring(0, 4)
        If Month = 1 Then
            year += 1
        ElseIf Month = 2 Then
            year += 1
        ElseIf Month = 3 Then
            year += 1
        End If

        If cbMonth.SelectedValue Is Nothing Then
            cbMonth.SelectedValue = Date.Now.Month
        End If

        Month = cbMonth.SelectedValue
        cbMonth.SelectedValue = Month
        dailyVMM.QuestionDayList.Clear()
        dailyVMM.Days.Clear()
        InitEmpAuditDailybyWeekData()

        If dailyVMM.Days.Count <> 0 Then
            GenerateQuestions()
        Else
            If e.RemovedItems.Count <> 0 Then
                If Not isInitialize Then
                    MsgBox("There is no records in selected date.  ", vbOKOnly + MsgBoxStyle.Exclamation, "AIDE")
                End If
            End If

        End If

        _logger.Debug("End : YearSelectionChanged")

    End Sub
End Class



