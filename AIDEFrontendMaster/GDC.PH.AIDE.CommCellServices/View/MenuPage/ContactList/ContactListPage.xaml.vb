﻿Imports UI_AIDE_CommCellServices.ServiceReference1
Imports System.Reflection
Imports System.IO
Imports System.Diagnostics
Imports System.ServiceModel
Imports System.Collections.ObjectModel
Imports System.Windows.Xps.Packaging
Imports System.Windows.Xps
Imports System.Printing
Imports Excel = Microsoft.Office.Interop.Excel

''' <summary>
''' By Aevan Camille Batongbacal
''' </summary>
''' <remarks></remarks>
<CallbackBehavior(ConcurrencyMode:=ConcurrencyMode.Single, UseSynchronizationContext:=False)>
Public Class ContactListPage
    Implements ServiceReference1.IAideServiceCallback

#Region "Paging Declarations"
    Private Enum PagingMode
        _First = 1
        _Next = 2
        _Previous = 3
        _Last = 4
    End Enum

    Dim startRowIndex As Integer
    Dim lastRowIndex As Integer
    Dim pagingPageIndex As Integer
    Dim pagingRecordPerPage As Integer = 10
#End Region

#Region "Fields"
    Private _AideService As ServiceReference1.AideServiceClient
    Private mainFrame As Frame
    Private isEmpty As Boolean
    Private email As String
    Private addframe As Frame
    Private menugrid As Grid
    Private submenuframe As Frame
    Private attendanceFrame As Frame
    Private profile As Profile

    Dim lstContacts As ContactList()
    Dim paginatedCollection As PaginatedObservableCollection(Of ContactListModel) = New PaginatedObservableCollection(Of ContactListModel)(pagingRecordPerPage)
#End Region

#Region "Constructor"

    Public Sub New(mainFrame As Frame, _profile As Profile, _addframe As Frame, _menugrid As Grid, _submenuframe As Frame, _attendanceFrame As Frame)

        InitializeComponent()
        Me.email = _profile.Email_Address
        Me.mainFrame = mainFrame
        Me.addframe = _addframe
        Me.menugrid = _menugrid
        Me.submenuframe = _submenuframe
        Me.attendanceFrame = _attendanceFrame
        Me.profile = _profile

        If profile.Permission = "Manager" Then
            btnCreate.Visibility = Windows.Visibility.Visible
        End If

        SetData()
        LoadData()
        'DisplayPagingInfo()
    End Sub

    Public Function InitializeService() As Boolean
        Dim bInitialize As Boolean = False
        Try
            Dim Context As InstanceContext = New InstanceContext(Me)
            _AideService = New AideServiceClient(Context)
            _AideService.Open()
            bInitialize = True
        Catch ex As SystemException
            _AideService.Abort()
        End Try
        Return bInitialize
    End Function

#End Region

#Region "Sub Procedures"

    Public Sub SetData()
        Try
            If InitializeService() Then
                lstContacts = _AideService.ViewContactListAll(email)
                'SetPaging(PagingMode._First)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Public Sub LoadData()
        Try
            Dim lstContactsList As New ObservableCollection(Of ContactListModel)
            Dim contactListDBProvider As New ContactListDBProvider
            Dim contactListVM As New ContactListViewModel()

            For Each objContacts As ContactList In lstContacts
                contactListDBProvider.SetMyContactList(objContacts)
            Next

            For Each contacts As MyContactList In contactListDBProvider.GetMyContactList()
                paginatedCollection.Add(New ContactListModel(contacts))
            Next

            lv_contacts.ItemsSource = paginatedCollection
            'contactListVM.ContactList = lstContactsList
            'lv_contacts.ItemsSource = lstContactsList
            'Me.DataContext = contactListVM
            LoadDataForPrint()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "FAILED")
        End Try
    End Sub

    Public Sub LoadDataForPrint()
        Try
            Dim lstContactsList As New ObservableCollection(Of ContactListModel)
            Dim contactListDBProvider As New ContactListDBProvider
            Dim contactListVM As New ContactListViewModel()

            Dim objContacts As New ContactList()

            For i As Integer = 0 To lstContacts.Length - 1
                objContacts = lstContacts(i)
                contactListDBProvider.SetMyContactList(objContacts)
            Next

            For Each rawUser As MyContactList In contactListDBProvider.GetMyContactList()
                lstContactsList.Add(New ContactListModel(rawUser))
            Next

            contactListVM.ContactListForPrint = lstContactsList

            dv_contacts.ItemsSource = lstContactsList

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "FAILED")
        End Try
    End Sub

    Private Sub SetPaging(mode As Integer)
        Try
            Dim totalRecords As Integer = lstContacts.Length

            Select Case mode
                Case CInt(PagingMode._Next)
                    ' Set the rows to be displayed if the total records is more than the (Record per Page * Page Index)
                    If totalRecords > (pagingPageIndex * pagingRecordPerPage) Then

                        ' Set the last row to be displayed if the total records is more than the (Record per Page * Page Index) + Record per Page
                        If totalRecords >= ((pagingPageIndex * pagingRecordPerPage) + pagingRecordPerPage) Then
                            lastRowIndex = ((pagingPageIndex * pagingRecordPerPage) + pagingRecordPerPage) - 1
                        Else
                            lastRowIndex = totalRecords - 1
                        End If

                        startRowIndex = pagingPageIndex * pagingRecordPerPage
                        pagingPageIndex += 1
                    Else
                        startRowIndex = (pagingPageIndex - 1) * pagingRecordPerPage
                        lastRowIndex = totalRecords - 1
                    End If
                    ' Bind data to the Data Grid
                    LoadData()
                    Exit Select
                Case CInt(PagingMode._Previous)
                    ' Set the Previous Page if the page index is greater than 1
                    If pagingPageIndex > 1 Then
                        pagingPageIndex -= 1

                        startRowIndex = ((pagingPageIndex * pagingRecordPerPage) - pagingRecordPerPage)
                        lastRowIndex = (pagingPageIndex * pagingRecordPerPage) - 1
                        LoadData()
                    End If
                    Exit Select
                Case CInt(PagingMode._First)
                    If totalRecords > pagingRecordPerPage Then
                        pagingPageIndex = 2
                        SetPaging(CInt(PagingMode._Previous))
                    Else
                        pagingPageIndex = 1
                        startRowIndex = ((pagingPageIndex * pagingRecordPerPage) - pagingRecordPerPage)

                        If Not totalRecords = 0 Then
                            lastRowIndex = totalRecords - 1
                            LoadData()
                        Else
                            lastRowIndex = 0
                            Me.DataContext = Nothing
                        End If

                    End If
                    Exit Select
                Case CInt(PagingMode._Last)
                    pagingPageIndex = (lstContacts.Length / pagingRecordPerPage)
                    SetPaging(CInt(PagingMode._Next))
                    Exit Select
            End Select

            DisplayPagingInfo()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "FAILED")
        End Try
    End Sub

    Private Sub DisplayPagingInfo()
        Dim pagingInfo As String

        ' If there has no data found
        If lstContacts.Length = 0 Then
            pagingInfo = "No Results Found "
            GUISettingsOff()
        Else
            pagingInfo = "Displaying " & startRowIndex + 1 & " to " & lastRowIndex + 1
            GUISettingsOn()
        End If
    End Sub

    Private Sub GUISettingsOff()
        lv_contacts.Visibility = Windows.Visibility.Hidden

        btnPrev.IsEnabled = False
        btnNext.IsEnabled = False
    End Sub

    Private Sub GUISettingsOn()
        lv_contacts.Visibility = Windows.Visibility.Visible

        btnPrev.IsEnabled = True
        btnNext.IsEnabled = True
    End Sub
#End Region

#Region "Events"
    Private Sub lv_contacts_LoadingRow(sender As Object, e As DataGridRowEventArgs) Handles lv_contacts.LoadingRow
        Dim RowDataContaxt As ContactListModel = TryCast(e.Row.DataContext, ContactListModel)
        If RowDataContaxt IsNot Nothing Then
            If RowDataContaxt.IsREVIEWED = False Then
                e.Row.Background = New BrushConverter().ConvertFrom("#CCFFD8D8")
            End If
        End If
    End Sub

    Private Sub lv_contacts_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles lv_contacts.MouseDoubleClick
        e.Handled = True
        If lv_contacts.SelectedIndex <> -1 Then
            If lv_contacts.SelectedItem IsNot Nothing Then
                Dim _Email As String = CType(lv_contacts.SelectedItem, ContactListModel).EMAIL_ADDRESS

                If profile.Permission = "Manager" Then
                    Dim contactList As New ContactListModel
                    contactList.EMP_ID = CType(lv_contacts.SelectedItem, ContactListModel).EMP_ID
                    contactList.FIRST_NAME = CType(lv_contacts.SelectedItem, ContactListModel).FIRST_NAME
                    contactList.LAST_NAME = CType(lv_contacts.SelectedItem, ContactListModel).LAST_NAME
                    contactList.MIDDLE_NAME = CType(lv_contacts.SelectedItem, ContactListModel).MIDDLE_NAME
                    contactList.NICK_NAME = CType(lv_contacts.SelectedItem, ContactListModel).NICK_NAME
                    contactList.ACTIVE = CType(lv_contacts.SelectedItem, ContactListModel).ACTIVE
                    contactList.BDATE = CType(lv_contacts.SelectedItem, ContactListModel).BDATE
                    contactList.POSITION = CType(lv_contacts.SelectedItem, ContactListModel).POSITION
                    contactList.DT_HIRED = CType(lv_contacts.SelectedItem, ContactListModel).DT_HIRED
                    contactList.MARITAL_STATUS = CType(lv_contacts.SelectedItem, ContactListModel).MARITAL_STATUS
                    contactList.IMAGE_PATH = CType(lv_contacts.SelectedItem, ContactListModel).IMAGE_PATH
                    contactList.PERMISSION_GROUP = CType(lv_contacts.SelectedItem, ContactListModel).PERMISSION_GROUP
                    contactList.DEPARTMENT = CType(lv_contacts.SelectedItem, ContactListModel).DEPARTMENT
                    contactList.DIVISION = CType(lv_contacts.SelectedItem, ContactListModel).DIVISION
                    contactList.SHIFT = CType(lv_contacts.SelectedItem, ContactListModel).SHIFT
                    contactList.EMAIL_ADDRESS = CType(lv_contacts.SelectedItem, ContactListModel).EMAIL_ADDRESS
                    contactList.EMAIL_ADDRESS2 = CType(lv_contacts.SelectedItem, ContactListModel).EMAIL_ADDRESS2
                    contactList.LOCATION = CType(lv_contacts.SelectedItem, ContactListModel).LOCATION
                    contactList.CEL_NO = CType(lv_contacts.SelectedItem, ContactListModel).CEL_NO
                    contactList.LOCAL = CType(lv_contacts.SelectedItem, ContactListModel).LOCAL
                    contactList.HOMEPHONE = CType(lv_contacts.SelectedItem, ContactListModel).HOMEPHONE
                    contactList.OTHER_PHONE = CType(lv_contacts.SelectedItem, ContactListModel).OTHER_PHONE
                    contactList.DT_REVIEWED = CType(lv_contacts.SelectedItem, ContactListModel).DT_REVIEWED

                    addframe.Navigate(New UpdateContactListPage(contactList, mainFrame, profile, addframe, menugrid, submenuframe, attendanceFrame))

                    ', mainFrame, profile, addframe, menugrid, submenuframe, attendanceFrame

                    'addframe.Navigate(New UpdateContactList(contactList, mainFrame, profile, addframe, menugrid, submenuframe, attendanceFrame))
                    mainFrame.IsEnabled = False
                    mainFrame.Opacity = 0.3
                    menugrid.IsEnabled = False
                    menugrid.Opacity = 0.3
                    submenuframe.IsEnabled = False
                    submenuframe.Opacity = 0.3
                    addframe.Visibility = Visibility.Visible
                    addframe.Margin = New Thickness(100, 80, 100, 80)
                Else
                    If email = _Email.ToLower Then
                        Dim contactList As New ContactListModel
                        contactList.EMP_ID = CType(lv_contacts.SelectedItem, ContactListModel).EMP_ID
                        contactList.FIRST_NAME = CType(lv_contacts.SelectedItem, ContactListModel).FIRST_NAME
                        contactList.LAST_NAME = CType(lv_contacts.SelectedItem, ContactListModel).LAST_NAME
                        contactList.MIDDLE_NAME = CType(lv_contacts.SelectedItem, ContactListModel).MIDDLE_NAME
                        contactList.NICK_NAME = CType(lv_contacts.SelectedItem, ContactListModel).NICK_NAME
                        contactList.ACTIVE = CType(lv_contacts.SelectedItem, ContactListModel).ACTIVE
                        contactList.BDATE = CType(lv_contacts.SelectedItem, ContactListModel).BDATE
                        contactList.POSITION = CType(lv_contacts.SelectedItem, ContactListModel).POSITION
                        contactList.DT_HIRED = CType(lv_contacts.SelectedItem, ContactListModel).DT_HIRED
                        contactList.MARITAL_STATUS = CType(lv_contacts.SelectedItem, ContactListModel).MARITAL_STATUS
                        contactList.IMAGE_PATH = CType(lv_contacts.SelectedItem, ContactListModel).IMAGE_PATH
                        contactList.PERMISSION_GROUP = CType(lv_contacts.SelectedItem, ContactListModel).PERMISSION_GROUP
                        contactList.DEPARTMENT = CType(lv_contacts.SelectedItem, ContactListModel).DEPARTMENT
                        contactList.DIVISION = CType(lv_contacts.SelectedItem, ContactListModel).DIVISION
                        contactList.SHIFT = CType(lv_contacts.SelectedItem, ContactListModel).SHIFT
                        contactList.EMAIL_ADDRESS = CType(lv_contacts.SelectedItem, ContactListModel).EMAIL_ADDRESS
                        contactList.EMAIL_ADDRESS2 = CType(lv_contacts.SelectedItem, ContactListModel).EMAIL_ADDRESS2
                        contactList.LOCATION = CType(lv_contacts.SelectedItem, ContactListModel).LOCATION
                        contactList.CEL_NO = CType(lv_contacts.SelectedItem, ContactListModel).CEL_NO
                        contactList.LOCAL = CType(lv_contacts.SelectedItem, ContactListModel).LOCAL
                        contactList.HOMEPHONE = CType(lv_contacts.SelectedItem, ContactListModel).HOMEPHONE
                        contactList.OTHER_PHONE = CType(lv_contacts.SelectedItem, ContactListModel).OTHER_PHONE
                        contactList.DT_REVIEWED = CType(lv_contacts.SelectedItem, ContactListModel).DT_REVIEWED

                        addframe.Navigate(New UpdateContactListPage(contactList, mainFrame, profile, addframe, menugrid, submenuframe, attendanceFrame))

                        ', mainFrame, profile, addframe, menugrid, submenuframe, attendanceFrame

                        'addframe.Navigate(New UpdateContactList(contactList, mainFrame, profile, addframe, menugrid, submenuframe, attendanceFrame))
                        mainFrame.IsEnabled = False
                        mainFrame.Opacity = 0.3
                        menugrid.IsEnabled = False
                        menugrid.Opacity = 0.3
                        submenuframe.IsEnabled = False
                        submenuframe.Opacity = 0.3
                        addframe.Visibility = Visibility.Visible
                        addframe.Margin = New Thickness(100, 80, 100, 80)
                    Else
                        Exit Sub
                    End If
                End If

                
            End If
        End If
    End Sub

    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
        Dim dialog As PrintDialog = New PrintDialog()

        If CBool(dialog.ShowDialog().GetValueOrDefault()) Then
            dv_contacts.Visibility = Windows.Visibility.Visible
            printBorder.Visibility = Windows.Visibility.Visible
            Dim pageSize As Size = New Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight)
            dv_contacts.Measure(pageSize)
            dv_contacts.Arrange(New Rect(5, 5, pageSize.Width, pageSize.Height))
            dialog.PrintVisual(dv_contacts, "Print Contacts")
        End If

        dv_contacts.Visibility = Windows.Visibility.Hidden
        printBorder.Visibility = Windows.Visibility.Hidden
    End Sub

    Private Sub btnCreate_Click(sender As Object, e As RoutedEventArgs)
        addframe.Navigate(New InsertContactList())
        mainFrame.IsEnabled = False
        mainFrame.Opacity = 0.3
        menugrid.IsEnabled = False
        menugrid.Opacity = 0.3
        submenuframe.IsEnabled = False
        submenuframe.Opacity = 0.3
        addframe.Visibility = Visibility.Visible
        addframe.Margin = New Thickness(100, 80, 100, 80)
    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        Dim totalRecords As Integer = lstContacts.Length

        If totalRecords >= ((paginatedCollection.CurrentPage * pagingRecordPerPage) + pagingRecordPerPage) Then
            paginatedCollection.CurrentPage = paginatedCollection.CurrentPage + 1
        End If
    End Sub

    Private Sub btnPrev_Click(sender As Object, e As RoutedEventArgs) Handles btnPrev.Click
        paginatedCollection.CurrentPage = paginatedCollection.CurrentPage - 1
    End Sub

    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs)
        SetPaging(CInt(PagingMode._First))
    End Sub

    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs)
        SetPaging(CInt(PagingMode._Last))
    End Sub
#End Region

#Region "ICallBack Function"
    Public Sub NotifyError(message As String) Implements IAideServiceCallback.NotifyError
        If message <> String.Empty Then
            MessageBox.Show(message)
        End If
    End Sub

    Public Sub NotifyOffline(EmployeeName As String) Implements IAideServiceCallback.NotifyOffline

    End Sub

    Public Sub NotifyPresent(EmployeeName As String) Implements IAideServiceCallback.NotifyPresent

    End Sub

    Public Sub NotifySuccess(message As String) Implements IAideServiceCallback.NotifySuccess
        If message <> String.Empty Then
            MessageBox.Show(message)
        End If
    End Sub

    Public Sub NotifyUpdate(objData As Object) Implements IAideServiceCallback.NotifyUpdate

    End Sub
#End Region

End Class
