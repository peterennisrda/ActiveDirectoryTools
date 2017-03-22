Option Compare Database
Option Explicit

' Constants for settings of "ADAAL"
Public Const gblnTEST As Boolean = True
Public Const gstrPROJECT_ADAAL As String = "ADAAL"
Private Const mstrVERSION_ADAAL As String = "0.0.0"
Private Const mstrDATE_ADAAL As String = "March 22, 2017"

Public Const THE_FRONT_END_APP = True
Public Const THE_SOURCE_FOLDER = ".\src\"
Public Const THE_XML_FOLDER = ".\src\xml\"
Public Const THE_XML_DATA_FOLDER = ".\src\xmldata\"
Public Const THE_BACK_END_SOURCE_FOLDER = "NONE"        ' ".\srcbe\"
Public Const THE_BACK_END_XML_FOLDER = ".\srcbe\xml\"
Public Const THE_BACK_END_DB1 = "NONE"                  ' "C:\SOME\LOCATION\FOR\BACKEND.accdb"
'

Public Function getMyVersion() As String
    On Error GoTo 0
    getMyVersion = mstrVERSION_ADAAL
End Function

Public Function getMyDate() As String
    On Error GoTo 0
    getMyDate = mstrDATE_ADAAL
End Function

Public Function getMyProject() As String
    On Error GoTo 0
    getMyProject = gstrPROJECT_ADAAL
End Function

Public Sub ADAAL_EXPORT(Optional ByVal varDebug As Variant)

    On Error GoTo PROC_ERR

    'Debug.Print "THE_BACK_END_DB1 = " & THE_BACK_END_DB1
    If Not IsMissing(varDebug) Then
        aegitClassTest varDebug:="varDebug", varSrcFldr:=THE_SOURCE_FOLDER, varSrcFldrBe:=THE_BACK_END_SOURCE_FOLDER, _
                        varXmlFldr:=THE_XML_FOLDER, varXmlDataFldr:=THE_XML_DATA_FOLDER, _
                        varFrontEndApp:=THE_FRONT_END_APP, _
                        varBackEndDbOne:=THE_BACK_END_DB1
    Else
        aegitClassTest varSrcFldr:=THE_SOURCE_FOLDER, varSrcFldrBe:=THE_BACK_END_SOURCE_FOLDER, _
                        varXmlFldr:=THE_XML_FOLDER, varXmlDataFldr:=THE_XML_DATA_FOLDER, _
                        varFrontEndApp:=THE_FRONT_END_APP, _
                        varBackEndDbOne:=THE_BACK_END_DB1
    End If

PROC_EXIT:
    Exit Sub

PROC_ERR:
    MsgBox "Erl=" & Erl & " Error " & Err.Number & " (" & Err.Description & ") in procedure ADAAL_EXPORT"
    Resume Next

End Sub

'=============================================================================================================================
' Tasks:
' %005 -
' %004 -
' %003 -
' %002 -
' %001 -
'=============================================================================================================================
'
'
'20170322 - v000
    ' Initial commit with new database and aegit export tool