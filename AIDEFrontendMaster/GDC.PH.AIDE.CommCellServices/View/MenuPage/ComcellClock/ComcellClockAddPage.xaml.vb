﻿Imports UI_AIDE_CommCellServices.ServiceReference1
Imports System.ServiceModel
Imports System.Text.RegularExpressions

<CallbackBehavior(ConcurrencyMode:=ConcurrencyMode.Single, UseSynchronizationContext:=False)>
Class ComcellClockAddPage
    Implements ServiceReference1.IAideServiceCallback
    Private empID As Integer
    Private comcellFrame As Frame
    Private aide As ServiceReference1.AideServiceClient
    Private _comcellclock As New ComcellClock
    Private comcellClockVM As New ComcellClockViewModel
    Private _window As Window
    Private pos As String


    Public Sub New(emp_id As Integer, com_cellframe As Frame, winx As Window, _pos As String)

        ' This call is required by the designer.
        InitializeComponent()
        Me.empID = emp_id
        Me.comcellFrame = com_cellframe
        Me._window = winx
        Me.pos = _pos
        populateDayCB()
        DataContext = comcellClockVM

        ' Add any initialization after the InitializeComponent() call.

    End Sub

#Region "Service Methods"
    Public Function InitializeService() As Boolean
        Dim bInitialize As Boolean = False
        Try
            Dim Context As InstanceContext = New InstanceContext(Me)
            aide = New AideServiceClient(Context)
            aide.Open()
            bInitialize = True
        Catch ex As SystemException
            aide.Abort()
        End Try
        Return bInitialize
    End Function
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
#End Region


    Private Sub SetDataDay()
        If Not ComcellDayCB.Text = String.Empty Then
            comcellClockVM.objectComcellClockSet.CLOCK_DAY = ConvertDays(ComcellDayCB.Text)
        End If
        If Not Me.empID = 0 Then
            comcellClockVM.objectComcellClockSet.EMP_ID = Me.empID
        End If
    End Sub

    Public Sub SetData(clockVM As ComcellClockModel)
        Try
            SetDataDay()
            If Not clockVM.CLOCK_DAY = 0 AndAlso Not clockVM.CLOCK_HOUR = 0 Then
                If checkLimit() Then
                    If InitializeService() Then
                        _comcellclock.Clock_Day = clockVM.CLOCK_DAY
                        _comcellclock.Clock_Hour = clockVM.CLOCK_HOUR
                        _comcellclock.Clock_Minute = clockVM.CLOCK_MINUTE
                        _comcellclock.Emp_ID = clockVM.EMP_ID
                        aide.UpdateComcellClock(_comcellclock)
                        MsgBox("Successfully set new Comm. Cell clock", MsgBoxStyle.OkOnly, "AIDE")
                        comcellFrame.Navigate(New ComcellClockPage(Me.empID, Me.comcellFrame, Me._window, Me.pos))
                    End If
                Else
                    MsgBox("Please check time entry. Hours should not exceed 24. Minutes should not exceed 59.", MsgBoxStyle.OkOnly, "AIDE")
                End If
            Else
                MsgBox("Please fill up all needed information.", MsgBoxStyle.OkOnly, "AIDE")
            End If
        Catch ex As Exception

        End Try
    End Sub


    Private Sub BackBtn_Click(sender As Object, e As RoutedEventArgs)
        comcellFrame.Navigate(New ComcellClockPage(Me.empID, Me.comcellFrame, _window, Me.pos))
    End Sub

    Public Sub populateDayCB()
        For count As Integer = 1 To 5
            ComcellDayCB.Items.Add(getDays(count))
        Next
    End Sub

    Private Function getDays(count As Integer) As String
        Select Case count
            Case 1
                Return "Monday"
            Case 2
                Return "Tuesday"
            Case 3
                Return "Wednesday"
            Case 4
                Return "Thursday"
            Case 5
                Return "Friday"
            Case Else
                Return String.Empty
        End Select
    End Function
    Private Function ConvertDays(days As String) As Integer
        Select Case days
            Case "Monday"
                Return 1
            Case "Tuesday"
                Return 2
            Case "Wednesday"
                Return 3
            Case "Thursday"
                Return 4
            Case "Friday"
                Return 5
            Case "Saturday"
                Return 6
            Case "Sunday"
                Return 7
        End Select
    End Function

    Private Function checkLimit() As Boolean
        checkLimit = False
        If comcellClockVM.objectComcellClockSet.CLOCK_HOUR > 0 And comcellClockVM.objectComcellClockSet.CLOCK_HOUR <= 24 Then
            If comcellClockVM.objectComcellClockSet.CLOCK_MINUTE >= 0 AndAlso comcellClockVM.objectComcellClockSet.CLOCK_MINUTE < 60 Then
                checkLimit = True
            End If
        End If
        Return checkLimit
    End Function

    Private Sub UpdateBtn_Click(sender As Object, e As RoutedEventArgs)
        Try
            SetData(comcellClockVM.objectComcellClockSet)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub ComcellMinuteCB_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
        Dim _regex As Regex = New Regex("[^0-9]+")
        e.Handled = _regex.IsMatch(e.Text)
    End Sub

    Private Sub ComcellHourCB_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
        Dim _regex As Regex = New Regex("[^0-9]+")
        e.Handled = _regex.IsMatch(e.Text)
    End Sub
End Class