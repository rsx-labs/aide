﻿Imports System.Data
Imports UI_AIDE_CommCellServices.ServiceReference1
Imports System.Collections.ObjectModel
Imports System.ServiceModel
Imports NLog
Class KPISummaryAddPage
    Implements UI_AIDE_CommCellServices.ServiceReference1.IAideServiceCallback


#Region "Page Declaration"
    Public _frame As Frame
    'Private aide As AideServiceClient
    Private comcell As New Comcell
    Private _kpiSummaryModel As New KPISummaryModel
    Private _addframe As Frame
    Private _menugrid As Grid
    Private _submenuframe As Frame
    Private profile As Profile
    Private nextYear As Integer
    Private mode As String
    Private comcellID As Integer
    Private dsplyByDiv As Integer = 1
    Dim _startYear As Integer
    Dim _endYear As Integer
    Dim _dtStartYear As Date
    Dim _dtEndYear As Date

    Dim _lstKPITargets As KPITargets()
    Dim _KPITargetsVM As New KPITargetsViewModel()
    Dim _kpiSummary As New KPISummary
    Private _dictMonths As New Dictionary(Of String, String)

    Private _logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger()
#End Region

#Region "Constructors"
    'Add Constructor
    Public Sub New(_profile As Profile, mainframe As Frame, addframe As Frame, menugrid As Grid, submenuframe As Frame)

        _logger.Debug("Start : Constructor Mode=Add")

        Try
            Me._frame = mainframe
            Me._addframe = addframe
            Me._menugrid = menugrid
            Me._submenuframe = submenuframe
            Me.profile = _profile
            InitializeComponent()

            'aide = aideService

            SetData()
            LoadMonth()
            LoadControls()
            mode = "Add"
        Catch ex As Exception
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : Constructor Mode=Add")
    End Sub

    'Update Constructor
    Public Sub New(_profile As Profile, mainframe As Frame, addframe As Frame, menugrid As Grid, submenuframe As Frame, kpi As KPISummary)

        _logger.Debug("Start : Constructor Mode=Update")

        Try
            Me._frame = mainframe
            Me._addframe = addframe
            Me._menugrid = menugrid
            Me._submenuframe = submenuframe
            Me.profile = _profile
            Me._kpiSummary = kpi

            InitializeComponent()
            LoadMonth()
            LoadYears(_kpiSummary.FYStart.Year, _kpiSummary.FYEnd.Year)
            'LoadControls()

            If Me._kpiSummary.KPI_Month >= 4 Then
                ' cbYear.Text = Convert.ToDateTime(_kpiSummary.FYStart).Year.ToString()
                txtBlockYear.Text = Convert.ToDateTime(_kpiSummary.FYStart).Year.ToString()
                cbYear.SelectedValue = Convert.ToDateTime(_kpiSummary.FYStart).Year.ToString()
            Else
                'cbYear.Text = Convert.ToDateTime(_kpiSummary.FYEnd).Year.ToString()
                txtBlockYear.Text = Convert.ToDateTime(_kpiSummary.FYEnd).Year.ToString()
                cbYear.SelectedValue = Convert.ToDateTime(_kpiSummary.FYEnd).Year.ToString()
            End If
            cbYear.IsEnabled = False

            cbKPI.Items.Add(New With {.Text = _kpiSummary.Subject, .Value = _kpiSummary.KPI_Reference})
            cbKPI.DisplayMemberPath = "Text"
            cbKPI.SelectedIndex = 0
            cbKPI.IsEnabled = False

            cbMonth.SelectedValue = _kpiSummary.KPI_Month
            cbMonth.IsEnabled = False
            txtActual.Text = _kpiSummary.KPIActual
            txtTarget.Text = _kpiSummary.KPITarget
            'LoadControls()

            txtHeader.Text = "Update KPI Summary"
            mode = "UPDATE"
            txtBlockButton.Text = mode
            AddBtn.Style = FindResource("RoundCornerUpdate")
        Catch ex As Exception
            _logger.Error(ex.ToString())

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try

        _logger.Debug("End : Constructor Mode=Update")

    End Sub
#End Region

#Region "Methods/Functions"
    'Public Function InitializeService() As Boolean
    '    Dim bInitialize As Boolean = False
    '    Try
    '        Dim Context As InstanceContext = New InstanceContext(Me)
    '        aide = New AideServiceClient(Context)
    '        aide.Open()
    '        bInitialize = True
    '    Catch ex As SystemException
    '        aide.Abort()
    '        MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
    '    End Try
    '    Return bInitialize
    'End Function


    Public Sub LoadControls()
        txtHeader.Text = "Monthly KPI Targets and Actual"
        txtBlockMonth.Text = Date.Now.Month
        txtBlockYear.Text = Date.Now.Year
        'txtBlockMonth.Text = ComcellModel.MONTH

        'txtBlockYear.Text = ComcellModel.FY_START

        cbMonth.SelectedValue = txtBlockMonth.Text
        cbYear.SelectedValue = txtBlockYear.Text
    End Sub

    Public Sub LoadMonth()
        cbMonth.DisplayMemberPath = "Text"
        cbMonth.SelectedValuePath = "Value"

        cbMonth.Items.Add(New With {.Text = "January", .Value = 1})
        cbMonth.Items.Add(New With {.Text = "February", .Value = 2})
        cbMonth.Items.Add(New With {.Text = "March", .Value = 3})
        cbMonth.Items.Add(New With {.Text = "April", .Value = 4})
        cbMonth.Items.Add(New With {.Text = "May", .Value = 5})
        cbMonth.Items.Add(New With {.Text = "June", .Value = 6})
        cbMonth.Items.Add(New With {.Text = "July", .Value = 7})
        cbMonth.Items.Add(New With {.Text = "August", .Value = 8})
        cbMonth.Items.Add(New With {.Text = "September", .Value = 9})
        cbMonth.Items.Add(New With {.Text = "October", .Value = 10})
        cbMonth.Items.Add(New With {.Text = "November", .Value = 11})
        cbMonth.Items.Add(New With {.Text = "December", .Value = 12})
    End Sub

    Public Sub LoadYears(ByVal startYr As Integer, ByVal endYr As Integer)
        Try
            cbYear.DisplayMemberPath = "Text"
            cbYear.SelectedValuePath = "Value"
            For i As Integer = startYr To endYr
                nextYear = i + 1
                cbYear.Items.Add(New With {.Text = i.ToString, .Value = i})
            Next
        Catch ex As Exception
            _logger.Error($"Error at LoadYears = {ex.ToString()}")

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try
    End Sub

    Public Sub SetData()
        Try
            'If InitializeService() Then
            'Dim fiscalYear As Date = Date.Now()

            _lstKPITargets = CommonUtility.Instance().KPITargetList 'AideClient.GetClient().GetAllKPITargets(Me.profile.Emp_ID, fiscalYear)

            LoadKPITargets()
            'End If
        Catch ex As Exception
            _logger.Error($"Error at SetData = {ex.ToString()}")

            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try
    End Sub

    Public Sub LoadKPITargets()
        Try
            Dim collKPITargets As New ObservableCollection(Of KPITargetsModel)
            Dim dbProvider As New KPITargetDBProvider
            Dim dict As Dictionary(Of String, String) = New Dictionary(Of String, String)
            Dim startYear As Integer
            Dim endYear As Integer

            For Each kpiTarget As KPITargets In _lstKPITargets
                dbProvider.SetKPITargets(kpiTarget)
            Next
            For Each kpi As KPITargetSet In dbProvider.GetAllKPITargets()
                dict.Add(kpi._KPI_RefNo, kpi._subject)
                startYear = kpi._FYStart.Year
                endYear = kpi._FYEnd.Year
                _dtStartYear = kpi._FYStart
                _dtEndYear = kpi._FYEnd
            Next

            LoadYears(startYear, endYear)

            cbKPI.ItemsSource = dict
            cbKPI.DisplayMemberPath = "Value"
            cbKPI.SelectedValuePath = "Key"
        Catch ex As Exception
            _logger.Error($"Error at LoadKPITargets = {ex.ToString()}")
            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try
    End Sub

    Private Sub ExitPage()
        _frame.Navigate(New KPISummaryPage(Me.profile, Me._frame, _addframe, _menugrid, _submenuframe))
        _frame.IsEnabled = True
        _frame.Opacity = 1
        _menugrid.IsEnabled = True
        _menugrid.Opacity = 1
        _submenuframe.IsEnabled = True
        _submenuframe.Opacity = 1
        _addframe.Visibility = Visibility.Hidden
    End Sub
#End Region

#Region "Events"
    Private Sub AddBtn_Click(sender As Object, e As RoutedEventArgs)
        Try
            If ValidateFields() = False Then
                MsgBox("Please enter all required fields. Ensure all required fields have * indicated.", vbOKOnly + MsgBoxStyle.Information, "AIDE")
                Exit Sub
            End If

            'If InitializeService() Then
            If Me.mode = "Add" Then
                Dim lstkpiSummary As KPISummary() = AideClient.GetClient().GetKPISummaryListMonthly(Me.profile.Emp_ID, _dtStartYear, _dtEndYear, cbMonth.SelectedValue, cbKPI.SelectedValue)
                If Not IsNothing(lstkpiSummary) Then
                    If lstkpiSummary.Count > 0 Then
                        MsgBox("KPI Summary already exists.", vbOKOnly + MsgBoxStyle.Information, "AIDE")
                        Exit Sub
                    Else
                        _kpiSummary.EmployeeId = Me.profile.Emp_ID
                        _kpiSummary.FYStart = _dtStartYear
                        _kpiSummary.FYEnd = _dtEndYear
                        _kpiSummary.KPI_Reference = cbKPI.SelectedValue
                        _kpiSummary.KPI_Month = cbMonth.SelectedValue
                        If CDbl(txtTarget.Text) = 0 Then
                            _kpiSummary.KPIActual = 0
                            _kpiSummary.KPITarget = 0
                            _kpiSummary.KPIOverall = 0

                        Else
                            _kpiSummary.KPIActual = Convert.ToDouble(CDbl(txtActual.Text) / 100)
                            _kpiSummary.KPITarget = Convert.ToDouble(CDbl(txtTarget.Text) / 100)
                            _kpiSummary.KPIOverall = Convert.ToDouble(CDbl(txtActual.Text) / CDbl(txtTarget.Text))
                        End If
                        _kpiSummary.DatePosted = Date.Now

                        If AideClient.GetClient().InsertKPISummary(_kpiSummary) = True Then
                            MsgBox("KPI Summary has been added.", vbOKOnly + MsgBoxStyle.Information, "AIDE")
                        End If
                    End If
                End If
            Else
                If CDbl(txtTarget.Text) = 0 Then
                    _kpiSummary.KPIActual = 0
                    _kpiSummary.KPITarget = 0
                    _kpiSummary.KPIOverall = 0

                Else
                    _kpiSummary.KPITarget = Convert.ToDouble(Convert.ToDouble(txtTarget.Text) / 100)
                    _kpiSummary.KPIActual = Convert.ToDouble(Convert.ToDouble(txtActual.Text) / 100)
                    _kpiSummary.KPIOverall = Convert.ToDouble(_kpiSummary.KPIActual / _kpiSummary.KPITarget)
                End If
                _kpiSummary.DatePosted = Date.Now

                If AideClient.GetClient().UpdateKPISummary(_kpiSummary) Then
                    MsgBox("KPI Summary has been updated.", vbOKOnly + MsgBoxStyle.Information, "AIDE")
                End If
            End If
            ExitPage()
            'End If

        Catch ex As Exception
            MsgBox("An application error was encountered. Please contact your AIDE Administrator.", vbOKOnly + vbCritical, "AIDE")
        End Try
    End Sub

    Private Sub BackBtn_Click(sender As Object, e As RoutedEventArgs)
        ExitPage()
    End Sub
#End Region

#Region "INotify Methods"
    Public Sub NotifyError(message As String) Implements IAideServiceCallback.NotifyError

    End Sub

    Public Sub NotifyOffline(EmployeeName As String) Implements IAideServiceCallback.NotifyOffline

    End Sub

    Public Sub NotifyPresent(EmployeeName As String) Implements IAideServiceCallback.NotifyPresent

    End Sub

    Public Sub NotifySuccess(message As String) Implements IAideServiceCallback.NotifySuccess

    End Sub

    Public Sub NotifyUpdate(objData As Object) Implements IAideServiceCallback.NotifyUpdate

    End Sub

    Private Sub txtActual_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)

    End Sub

    Private Sub txtTarget_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)

    End Sub

    Private Sub CbMonth_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbMonth.SelectionChanged

        If cbMonth.SelectedValue >= 1 And cbMonth.SelectedValue <= 3 Then
            cbYear.SelectedIndex = 1
        Else
            cbYear.SelectedIndex = 0
        End If
    End Sub

    Private Sub CbYear_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbYear.SelectionChanged
        If cbYear.SelectedIndex = 1 Then
            If cbMonth.SelectedIndex >= 4 Then
                cbMonth.SelectedIndex = 0
            End If
        End If
    End Sub

    Private Sub txtActual_KeyDown(sender As Object, e As KeyEventArgs) Handles txtActual.KeyDown
        If IsNumeric(e.Key) Then
            e.Handled = False
            'Dim dValue As Decimal = 0D
            'Decimal.TryParse(txtActual.Text, dValue)
            'txtActual.Text = dValue.ToString("N2")
        Else
            e.Handled = True
        End If
    End Sub

    Private Sub txtTarget_KeyDown(sender As Object, e As KeyEventArgs) Handles txtTarget.KeyDown
        If IsNumeric(e.Key) Then
            e.Handled = False

        Else
            e.Handled = True
        End If
    End Sub

    Private Sub txtTarget_KeyUp(sender As Object, e As KeyEventArgs) Handles txtTarget.KeyUp

    End Sub

    Private Sub txtActual_KeyUp(sender As Object, e As KeyEventArgs) Handles txtActual.KeyUp

    End Sub

    Function ValidateFields() As Boolean
        If IsNumeric(cbMonth.SelectedValue) And IsNumeric(cbYear.SelectedValue) And Not IsNothing(cbKPI.SelectedValue) And txtActual.Text <> String.Empty And txtTarget.Text <> String.Empty Then
            Return True
        End If
        Return False
    End Function

    Private Sub txtTarget_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtTarget.LostFocus

    End Sub

    Private Sub txtActual_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtActual.LostFocus

    End Sub

    Private Sub txtTarget_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles txtTarget.SelectionChanged
        Dim dValue As Decimal = 0D
        Decimal.TryParse(txtTarget.Text, dValue)
        txtTarget.Text = dValue.ToString("N2")
    End Sub

    Private Sub txtActual_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles txtActual.SelectionChanged
        Dim dValue As Decimal = 0D
        Decimal.TryParse(txtActual.Text, dValue)
        txtActual.Text = dValue.ToString("N2")
    End Sub


#End Region

End Class
