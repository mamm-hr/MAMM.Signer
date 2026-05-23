Attribute VB_Name = "MWin32"
Option Explicit

Private Type API_BROWSEINFO
    hwndOwner As Long
    pidlRoot As Long
    pszDisplayName As String
    lpszTitle As String
    ulFlags As Long
    lpfn As Long
    lParam As Long
    iImage As Long
End Type

Private Type API_OPENFILENAME
    lStructSize As Long
    hwndOwner As Long
    hInstance As Long
    lpstrFilter As String
    lpstrCustomFilter As String
    nMaxCustomFilter As Long
    nFilterIndex As Long
    lpstrFile As String
    nMaxFile As Long
    lpstrFileTitle As String
    nMaxFileTitle As Long
    lpstrInitialDir As String
    lpstrTitle As String
    Flags As Long
    nFileOffset As Integer
    nFileExtension As Integer
    lpstrDefExt As String
    lCustData As Long
    lpfnHook As Long
    lpTemplateName As String
End Type

Private Const MAX_PATH As Integer = 512 ' 260, doista.

Private Const API_OFN_HIDEREADONLY = &H4
Private Const API_OFN_OVERWRITEPROMPT = &H2
Private Const API_OFN_ALLOWMULTISELECT = &H200
Private Const API_OFN_PATHMUSTEXIST = &H800
Private Const API_OFN_FILEMUSTEXIST = &H1000
Private Const API_OFN_EXPLORER = &H80000

Private Const API_BIF_EDITBOX = &H10
Private Const API_BIF_NEWDIALOGSTYLE = &H40

Private Declare Function API_GetSaveFileName Lib "comdlg32.dll" Alias "GetSaveFileNameA" (lpofn As API_OPENFILENAME) As Long
Private Declare Function API_GetOpenFileName Lib "comdlg32.dll" Alias "GetOpenFileNameA" (lpofn As API_OPENFILENAME) As Long
Private Declare Function API_lstrlen Lib "kernel32" Alias "lstrlenA" (ByVal lpString As String) As Long
Private Declare Function API_SHBrowseForFolder Lib "shell32.dll" Alias "SHBrowseForFolderA" (lpbi As API_BROWSEINFO) As Long
Private Declare Function API_SHGetPathFromIDList Lib "shell32.dll" Alias "SHGetPathFromIDListA" (ByVal pidl As Long, ByVal pszPath As String) As Long

Public Function ShowBrowseForFolder( _
    ByVal frmParent As Form _
  , ByRef strFolderName As String _
  ) As Boolean
    
    Static pidlLast As Long
        
    Let ShowBrowseForFolder = False
        
    Dim bi As API_BROWSEINFO
    With bi
        Let .hwndOwner = frmParent.hWnd
        Let .pidlRoot = 0
        Let .pszDisplayName = String(MAX_PATH, vbNullChar) & vbNullChar
        Let .lpszTitle = "Izaberi mapu"
        Let .ulFlags = API_BIF_NEWDIALOGSTYLE
        Let .lpfn = 0
        Let .lParam = 0
        Let .iImage = 0
    End With
    
    Dim pidl As Long
    Let pidl = API_SHBrowseForFolder(bi)
    If 0 <> pidl Then
        If SHGetPathFromIDList(pidl, strFolderName) Then
            Let pidlLast = pidl
            Let ShowBrowseForFolder = True
        End If
    End If
      
End Function

' strFilters = "PDF-datoteke (*.pdf)|*.pdf|Sve datoteke (*.*)|*.*"
'
Public Function ShowSaveFileName( _
    ByVal strFilters As String _
  , ByVal frmParent As Form _
  , ByRef strFileName As String _
  , ByRef nFilterIndex As Integer _
  ) As Boolean

    Let ShowSaveFileName = False
    
    Dim ofn As API_OPENFILENAME
    With ofn
        Let .lStructSize = Len(ofn)
        Let .hwndOwner = frmParent.hWnd
        Let .hInstance = 0
        Let .lpstrFilter = Replace(strFilters, "|", vbNullChar) & String(2, vbNullChar)
        Let .nFilterIndex = 1
        Let .nMaxCustomFilter = 0
        Let .lpstrFile = String(MAX_PATH, vbNullChar) & vbNullChar
        Let .nMaxFile = Len(.lpstrFile)
        Let .lpstrFileTitle = Space(MAX_PATH) & vbNullChar
        Let .nMaxFileTitle = Len(.lpstrFileTitle)
        Let .lpstrInitialDir = vbNullChar
        Let .lpstrTitle = "Spremi kao"
        Let .Flags = API_OFN_PATHMUSTEXIST Or API_OFN_HIDEREADONLY Or API_OFN_OVERWRITEPROMPT
        Let .nFileOffset = 0
        Let .nFileExtension = 0
        Let .lCustData = 0
        Let .lpfnHook = 0
    End With
    
    If 0 <> API_GetSaveFileName(ofn) Then
        Let strFileName = Left(ofn.lpstrFile, InStr(ofn.lpstrFile, vbNullChar) - 1)
        Let nFilterIndex = IIf(0 = ofn.nFileExtension, ofn.nFilterIndex, 0)
        Let ShowSaveFileName = True
    End If
    
End Function

' strFilters = "PDF-datoteke (*.pdf)|*.pdf|Sve datoteke (*.*)|*.*"
'
Public Function ShowOpenFileName( _
    ByVal strFilters As String _
  , ByVal frmParent As Form _
  , ByVal nFilterIndex As Integer _
  , ByRef strFileName As String _
  ) As Boolean

    Let ShowOpenFileName = False
    
    Dim ofn As API_OPENFILENAME
    Call InitOpenFileName(ofn, strFilters, frmParent, nFilterIndex, bAllowMultiSelect:=False)
    
    If 0 <> API_GetOpenFileName(ofn) Then
        Let strFileName = Left(ofn.lpstrFile, InStr(ofn.lpstrFile, vbNullChar) - 1)
        Let ShowOpenFileName = True
    End If
    
End Function

' strFilters = "PDF-datoteke (*.pdf)|*.pdf|Sve datoteke (*.*)|*.*"
'
Public Function ShowOpenFileNames( _
    ByVal strFilters As String _
  , ByVal frmParent As Form _
  , ByVal nFilterIndex As Integer _
  , ByRef strDirPath As String _
  , ByRef astrFileName() As String _
  ) As Boolean

    Let ShowOpenFileNames = False
    
    Dim ofn As API_OPENFILENAME
    Call InitOpenFileName(ofn, strFilters, frmParent, nFilterIndex, bAllowMultiSelect:=True)
    
    If 0 <> API_GetOpenFileName(ofn) Then
        Let strDirPath = Left(ofn.lpstrFile, ofn.nFileOffset - 1)
        Dim strFileNames As String
        Let strFileNames = Mid(ofn.lpstrFile, ofn.nFileOffset + 1, InStr(ofn.lpstrFile, vbNullChar & vbNullChar) - 1 - ofn.nFileOffset)
        Let astrFileName = Split(strFileNames, vbNullChar)
        Let ShowOpenFileNames = True
    End If
    
End Function

Private Sub InitOpenFileName( _
    ByRef ofn As API_OPENFILENAME _
  , ByVal strFilters As String _
  , ByVal frmParent As Form _
  , ByVal nFilterIndex As Integer _
  , ByVal bAllowMultiSelect As Boolean _
  )
    With ofn
        Let .lStructSize = Len(ofn)
        Let .hwndOwner = frmParent.hWnd
        Let .hInstance = 0
        Let .lpstrFilter = Replace(strFilters, "|", vbNullChar) & String(2, vbNullChar)
        Let .nFilterIndex = nFilterIndex
        Let .nMaxCustomFilter = 0
        Let .lpstrFile = String(MAX_PATH, vbNullChar) & vbNullChar
        Let .nMaxFile = Len(.lpstrFile)
        Let .lpstrFileTitle = String(256, vbNullChar) & vbNullChar
        Let .nMaxFileTitle = Len(.lpstrFileTitle)
        Let .lpstrInitialDir = vbNullChar
        Let .lpstrTitle = "Otvori"
        Let .Flags = API_OFN_PATHMUSTEXIST Or API_OFN_FILEMUSTEXIST Or API_OFN_EXPLORER Or IIf(bAllowMultiSelect, API_OFN_ALLOWMULTISELECT, 0)
        Let .nFileOffset = 0
        Let .nFileExtension = 0
        Let .lCustData = 0
        Let .lpfnHook = 0
    End With
End Sub

Public Function SHGetPathFromIDList( _
    ByVal pidl As Long _
  , ByRef strPath _
  ) As Boolean
    Let SHGetPathFromIDList = False
    Dim strT As String: Let strT = String(MAX_PATH, vbNullChar) & vbNullChar
    If 0 <> API_SHGetPathFromIDList(pidl, strT) Then
        Let strPath = Left(strT, InStr(strT, vbNullChar) - 1)
        Let SHGetPathFromIDList = True
    End If
End Function
