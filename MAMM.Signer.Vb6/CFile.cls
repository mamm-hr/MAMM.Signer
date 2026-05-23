VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CFile"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
Option Explicit

Private m_nFile As Integer
Private m_bIsOpen As Boolean
Private m_strPath As String

Public Sub DeleteFile( _
    ByVal strFilePath As String _
  , Optional ByVal bMayNotExist As Boolean = False _
  )
    If bMayNotExist Then
        If Not Me.Exists(strFilePath) Then Exit Sub
    End If
    Call Kill(strFilePath)
End Sub

Public Function Exists( _
    ByVal strFilePath As String _
  )
    Let Exists = "" <> Dir(strFilePath)
End Function

Public Function IsValidName( _
    ByVal strName As String _
  ) As Boolean
    Let IsValidName = False
    Const INVALID As String = "<>:""/\|?*"
    Dim i As Integer
    For i = 1 To Len(INVALID)
        If 0 <> InStrRev(strName, Mid(INVALID, i, 1)) Then Exit Function
    Next i
    Let IsValidName = True
End Function


Public Function HasExt( _
    ByVal strFilePath As String _
  , ByRef strExt As String _
  ) As Boolean
    Let HasExt = Right(strFilePath, Len(strExt)) = strExt
End Function

Public Function ReadByteArray( _
    ByVal strFilePath As String _
  ) As Byte()

    If m_bIsOpen Then Close #m_nFile
    Let m_nFile = FreeFile()

    Open strFilePath For Binary Access Read As #m_nFile
    Let m_bIsOpen = True
    Let m_strPath = strFilePath
    
    Dim abyData() As Byte
    If 0 = LOF(m_nFile) Then
        Call Err.Raise(vbObjectError, , Replace("Praznu datoteku nije moguæe oèitati kao binarni sadržaj: '|1'.", "|1", strFilePath))
    End If
    ReDim abyData(LOF(m_nFile) - 1)
    
    Get #m_nFile, , abyData
    
    Close #m_nFile
    Let m_bIsOpen = False

    Let ReadByteArray = abyData

End Function

Public Function ReplaceDir( _
    ByVal strFilePath As String _
  , ByVal strDir As String _
  ) As String
    Let ReplaceDir = strFilePath
    If "" = strDir Then Exit Function
    Dim strFileName As String: strFileName = Dir(strFilePath)
    If "" = strFileName Then Exit Function
    Let ReplaceDir = strDir & "\" & strFileName
End Function

Public Function ReplaceExt( _
    ByVal strFilePath As String _
  , ByVal strExt As String _
  ) As String
    Let ReplaceExt = strFilePath
    Dim nAt As Integer: Let nAt = InStrRev(strFilePath, ".")
    If nAt < 1 Then Exit Function
    If Not Me.IsValidName(Mid(strFilePath, nAt)) Then Exit Function
    Let ReplaceExt = Left(strFilePath, nAt - 1) & strExt
End Function

Public Sub WriteByteArray( _
      ByVal strFilePath As String _
    , ByRef abyData() As Byte _
    )
      
    If m_bIsOpen Then Close #m_nFile
    Let m_nFile = FreeFile()

    Call Me.DeleteFile(strFilePath, bMayNotExist:=True)
    
    Open strFilePath For Binary Access Write As m_nFile
    Let m_bIsOpen = True
    Let m_strPath = strFilePath
    
    Put #m_nFile, , abyData
    
    Close #m_nFile
    Let m_bIsOpen = False

End Sub

Private Sub Class_Terminate()
    On Error Resume Next
    If m_bIsOpen Then Close #m_nFile
End Sub
