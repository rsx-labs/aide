﻿Imports System.Collections.ObjectModel
Imports UI_AIDE_CommCellServices.ServiceReference1

Public Class AnnouncementDBProvider
    Private _objAnnouncement As ObservableCollection(Of myAnnouncementSet)

    Public Sub New()
        _objAnnouncement = New ObservableCollection(Of myAnnouncementSet)
    End Sub

    Public Function _getobjAnnouncement() As ObservableCollection(Of myAnnouncementSet)
        Return _objAnnouncement
    End Function

    Public Function _setlistofitems(ByRef announce As Announcements)
        Dim _myAnnouncementSet As New myAnnouncementSet With {._announcementID = announce.ANNOUNCEMENT_ID, _
                                                ._empid = announce.EMP_ID, _
                                                  ._message = announce.MESSAGE, _
                                                  ._title = announce.TITLE, _
                                                  ._enddate = announce.END_DATE, _
                                                  ._deletedfg = announce.DELETED_FG}

        _objAnnouncement.Add(_myAnnouncementSet)
        Return _myAnnouncementSet
    End Function

End Class

Public Class myAnnouncementSet
    Property _announcementID As Integer
    Property _empid As Integer
    Property _message As String
    Property _title As String
    Property _enddate As Date
    Property _deletedfg As Integer
End Class