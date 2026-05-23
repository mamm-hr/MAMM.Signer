VERSION 5.00
Begin VB.Form FMainWindow 
   BorderStyle     =   3  'Fixed Dialog
   Caption         =   "MAMM Signer"
   ClientHeight    =   5700
   ClientLeft      =   45
   ClientTop       =   390
   ClientWidth     =   7335
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   5700
   ScaleWidth      =   7335
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton m_cmdOptions 
      Caption         =   "Opcije..."
      Height          =   375
      Left            =   4440
      TabIndex        =   7
      Top             =   5160
      Width           =   1335
   End
   Begin VB.CommandButton m_cmdExit 
      Caption         =   "Izlaz"
      Height          =   375
      Left            =   5880
      TabIndex        =   8
      Top             =   5160
      Width           =   1335
   End
   Begin VB.CommandButton m_cmdVerify 
      Caption         =   "Ovjeri"
      Height          =   375
      Left            =   3000
      TabIndex        =   6
      Top             =   5160
      Width           =   1335
   End
   Begin VB.CommandButton m_cmdSign 
      Caption         =   "Potpiši"
      Height          =   375
      Left            =   1560
      TabIndex        =   5
      Top             =   5160
      Width           =   1335
   End
   Begin VB.CommandButton m_cmdLoad 
      Caption         =   "Uèitaj..."
      Height          =   375
      Left            =   120
      TabIndex        =   4
      Top             =   5160
      Width           =   1335
   End
   Begin VB.TextBox m_txtDetails 
      Height          =   2895
      Left            =   120
      Locked          =   -1  'True
      MultiLine       =   -1  'True
      ScrollBars      =   3  'Both
      TabIndex        =   3
      Top             =   2040
      Width           =   7095
   End
   Begin VB.ListBox m_lstFiles 
      Height          =   1035
      Left            =   120
      TabIndex        =   1
      Top             =   480
      Width           =   7095
   End
   Begin VB.Label m_lblT 
      AutoSize        =   -1  'True
      Caption         =   "Detalji potpisa:"
      Height          =   195
      Index           =   1
      Left            =   120
      TabIndex        =   2
      Top             =   1680
      Width           =   1035
   End
   Begin VB.Label m_lblT 
      AutoSize        =   -1  'True
      Caption         =   "Popis datoteka:"
      Height          =   195
      Index           =   0
      Left            =   120
      TabIndex        =   0
      Top             =   120
      Width           =   1110
   End
End
Attribute VB_Name = "FMainWindow"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Private Enum OpConstants
    None
    Signed
    Verified
End Enum

Private m_colFile As Collection
Private m_oAppOptions As New CAppOptions
Private m_oPkcs As Pkcs7

Private Sub Form_Initialize()
    Set m_oPkcs = MammSignerLib.CreatePkcs7()
End Sub

Private Sub Form_Load()
    Call UiUpdateState
End Sub

' Izlaz iz programa.
Private Sub m_cmdExit_Click()
    Call Unload(Me)
End Sub

' Selektira datoteke i napuni popis datoteka njihovim stazama.
Private Sub m_cmdLoad_Click()
    On Error GoTo LBL_FAIL

    Dim strDirPath As String
    Dim astrFileName() As String
    Dim strFilters As String
    Let strFilters = Replace("Sve datoteke (*.*)|*.*|PKCS #7 ([filter])|[filter]", "[filter]", "*" & m_oAppOptions.Ext)
    If ShowOpenFileNames(strFilters, Me, 1, strDirPath, astrFileName) Then
        Call m_lstFiles.Clear
        Set m_colFile = New Collection
        Let m_txtDetails.Text = ""
        Dim i As Integer
        For i = LBound(astrFileName) To UBound(astrFileName)
            Call m_lstFiles.AddItem(strDirPath & "\" & astrFileName(i))
            Call m_colFile.Add(Array(OpConstants.None), Str(m_lstFiles.ListCount - 1))
        Next i
    End If
    
    Call UiUpdateState
    Exit Sub
LBL_FAIL:
    Call MsgBox(Err.Description, vbCritical)
    Call m_lstFiles.Clear
    Call UiUpdateState
End Sub

' Pokaže dijaloški okvir programskih opcija.
Private Sub m_cmdOptions_Click()
    With New FOptionsDialog
        Set .AppOptions = m_oAppOptions
        Set .Pkcs7Options = m_oPkcs.Options
        Call .ShowModal(Me)
    End With
End Sub

' Potpiše i opcionalno šifrira izabrane datoteke.
Private Sub m_cmdSign_Click()
    On Error GoTo LBL_FAIL
   
    ' Traži korisnika da izabere certifikat za potpisivanje.
    Dim oSignCert As MammSignerLib.Certificate
    If Not SelectCertificate(m_oAppOptions.SignLoc, CertificatePurpose.Signature, m_oAppOptions.SignCert, "Potpisivanje", "Izaberite ceritifikat za potpisivanje.", oSignCert) Then
        Call MsgBox("Certifikat za potpisivanje nije izabran.")
        Exit Sub
    End If
    
    ' Pita korisnika želi li šifrirati datoteku.
    Dim bSelectEncryptCert As Boolean: Let bSelectEncryptCert = True
    If "" = m_oAppOptions.EncryptCert Then
        Let bSelectEncryptCert = vbYes = MsgBox("Želite li izabrati certifikat za šifriranje?", vbYesNo, "Šifriranje")
    End If
    Dim oEncryptCert As MammSignerLib.Certificate
    Set oEncryptCert = Nothing
    If bSelectEncryptCert Then
        If Not SelectCertificate(m_oAppOptions.EncryptLoc, CertificatePurpose.Identification, m_oAppOptions.EncryptCert, "Šifriranje", "Izaberite certifikat za šifriranje", oEncryptCert) Then
            Call MsgBox("Certifikat za šifriranje nije izabran.")
            Exit Sub
        End If
    End If
    
    ' Potpisuje i opcionalno šifrira datoteke iz popisa koje nemaju PKCS #7 ekstenziju.
    With New CUiHourglass
    With MammSignerLib.CreatePkcs7()
        
        Dim i As Integer
        For i = 0 To m_lstFiles.ListCount - 1
        
            Dim strFilePath As String
            Let strFilePath = m_lstFiles.List(i)
                        
            Dim bFailed As Boolean
            Dim strResult As String
            On Error GoTo LBL_FAIL_OP
            Let bFailed = False
            
            ' Je li nedopuštena ekstenzija?
            With New CFile
                If .HasExt(strFilePath, m_oAppOptions.Ext) Then
                    Call Err.Raise(vbObjectError, , "Datoteka ima PKCS #7 ekstenziju.")
                End If
            End With
            
            ' Èita datoteku iz popisa.
            Dim abyData() As Byte
            With New CFile
                Let abyData = .ReadByteArray(strFilePath)
            End With
            
            ' Potpiše.
            Dim dtmNow As Date
            Let dtmNow = Now()
            Let abyData = .SignData(abyData, oSignCert, dtmNow)
            
            ' Šifrira.
            If Not Nothing Is oEncryptCert Then
                Let abyData = .EnvelopeData(abyData, oEncryptCert, m_oAppOptions.EncryptAlg)
            End If
                                    
            ' Kreira izlaznu datoteku.
            With New CFile
                Let strResult = .ReplaceDir(strFilePath, m_oAppOptions.OutDir) & m_oAppOptions.Ext
                Call .WriteByteArray(strResult, abyData)
            End With
            
LBL_CONTINUE_OP:
            On Error GoTo LBL_FAIL
            
            ' Spremi u kolekciju ishoda operacija ishod ove stavke popisa datoteka.
            Call m_colFile.Remove(Str(i))
            Call m_colFile.Add(Array(OpConstants.Signed, bFailed, strResult, oSignCert, dtmNow, oEncryptCert), Str(i))
            
        Next i
        
    End With
    End With

    Call UiUpdateState
    Exit Sub
LBL_FAIL_OP:
    Let bFailed = True
    Let strResult = Err.Description
    Resume LBL_CONTINUE_OP
LBL_FAIL:
    Call MsgBox(Err.Description, vbCritical)
    Call UiUpdateState
End Sub

' Ovjeri izabrane datoteke.
Private Sub m_cmdVerify_Click()
    On Error GoTo LBL_FAIL
    
    ' Pita korisnika želi li izabrati certifikat primatelja kojim æe se dešifrirati kuvertirane datoteke.
    Dim bSelectCert As Boolean: Let bSelectCert = True
    If "" = m_oAppOptions.EncryptCert Then
        Let bSelectCert = vbYes = MsgBox("Želite li izabrati certifikat za dešifriranje?", vbYesNo, "Dešifriranje")
    End If
    Dim oEncryptCert As MammSignerLib.Certificate: Set oEncryptCert = Nothing
    If bSelectCert Then
        If Not SelectCertificate(m_oAppOptions.EncryptLoc, CertificatePurpose.Identification, m_oAppOptions.EncryptCert, "Dešifriranje", "Izaberite certifikat za dešifriranje.", oEncryptCert) Then
            Call MsgBox("Certifikat za dešifriraanje nije izabran.")
            Exit Sub
        End If
    End If
       
    ' Ovjeri one datoteke iz popisa koje imaju PKCS #7 ekstenziju i ako su ovjerene, njihov sadržaj spramiu izlazni
    ' direktorij.
    With New CUiHourglass
    With MammSignerLib.CreatePkcs7()
        
        Dim i As Integer
        For i = 0 To m_lstFiles.ListCount - 1
                    
            Dim strFilePath As String
            Let strFilePath = m_lstFiles.List(i)
            
            Dim bDecrypted As Boolean
            Dim bVerified As Boolean
            Dim bFailed As Boolean
            Dim strResult As String
            On Error GoTo LBL_FAIL_OP
            Let bFailed = False
            
            ' Je li nedopuštena ekstenzija?
            With New CFile
                If Not .HasExt(strFilePath, m_oAppOptions.Ext) Then
                    Call Err.Raise(vbObjectError, , "Datoteka nema PKCS #7 ekstenziju.")
                End If
            End With
                    
            ' Èita datoteku.
            Dim abyData() As Byte
            With New CFile
                Let abyData = .ReadByteArray(m_lstFiles.List(i))
            End With
                
            ' Otvara kuveritrani sadržaj.
            If MammSignerLib.Oids.EnvelopedDataOid = .GetContentTypeOid(abyData) Then
                Let bDecrypted = True
                Let abyData = .OpenEnvelopedData(abyData, oEncryptCert)
            Else
                Let bDecrypted = False
            End If
            
            ' Ovjerava potpisani sadržaj.
            If MammSignerLib.Oids.SignedDataOid = .GetContentTypeOid(abyData) Then
                Let bVerified = True
                Let abyData = .VerifySignedData(abyData)
            Else
                Let bVerified = False
            End If
                    
            ' Kreira izlaznu datoteku.
            With New CFile
                Let strResult = .ReplaceDir(.ReplaceExt(strFilePath, ""), m_oAppOptions.OutDir)
                Call .WriteByteArray(strResult, abyData)
            End With
            
LBL_CONTINUE_OP:
            On Error GoTo LBL_FAIL
                                                
            ' Spremi u kolekciju ishoda operacija ishod ove stavke popisa datoteka.
            Call m_colFile.Remove(Str(i))
            Call m_colFile.Add(Array(OpConstants.Verified, bFailed, strResult, bDecrypted, oEncryptCert, bVerified), Str(i))
        
        Next i
        
    End With
    End With
    
    Call UiUpdateState
    Exit Sub
LBL_FAIL_OP:
    Let bFailed = True
    Let strResult = Err.Description
    Resume LBL_CONTINUE_OP
LBL_FAIL:
    Call MsgBox(Err.Description, vbCritical)
    Call UiUpdateState
End Sub

Private Sub m_lstFiles_Click()
    On Error GoTo LBL_FAIL
    
    If m_lstFiles.ListIndex < 0 Then
        Let m_txtDetails.Text = ""
        Exit Sub
    End If
    
    Dim bFailed As Boolean
    Dim strResult As String
    Dim oCert As MammSignerLib.Certificate
    
    Dim vaT As Variant
    Let vaT = m_colFile.Item(Str(m_lstFiles.ListIndex))
    Select Case vaT(LBound(vaT) + 0)
        
        Case OpConstants.None
            Let m_txtDetails.Text = "Datoteka je odabrana, ali još nije obraðena."
        
        Case OpConstants.Signed
            Let m_txtDetails.Text = ""
            Let bFailed = vaT(LBound(vaT) + 1)
            Let strResult = vaT(LBound(vaT) + 2)
            If bFailed Then
                Let m_txtDetails.Text = m_txtDetails.Text & _
                    "Potpisivanje datoteke nije uspjelo." & vbCrLf & _
                    "Razlog: " & strResult & vbCrLf & _
                    "" & vbCrLf
            Else
                Let m_txtDetails.Text = m_txtDetails.Text & _
                    "Potpisivanje datoteke je uspjelo." & vbCrLf & _
                    "Pohranjena datoteka: " & strResult & vbCrLf & _
                    "" & vbCrLf
                Set oCert = vaT(LBound(vaT) + 3)
                Let m_txtDetails.Text = m_txtDetails.Text & _
                    "DIGITALNI POTPIS" & vbCrLf
                GoSub LBL_OUTPUT_CERT
                Let m_txtDetails.Text = m_txtDetails.Text & _
                    "Vrijeme potpisa: '" & vaT(LBound(vaT) + 4) & "'" & vbCrLf & _
                    "" & vbCrLf
                Set oCert = vaT(LBound(vaT) + 5)
                If Not Nothing Is oCert Then
                    Let m_txtDetails.Text = m_txtDetails.Text & _
                        "ŠIFRIRANJE" & vbCrLf
                    GoSub LBL_OUTPUT_CERT
                    Let m_txtDetails.Text = m_txtDetails.Text & _
                        "" & vbCrLf
                End If
            End If
            
        Case OpConstants.Verified
            Let m_txtDetails.Text = ""
            Let bFailed = vaT(LBound(vaT) + 1)
            Let strResult = vaT(LBound(vaT) + 2)
            If bFailed Then
                Let m_txtDetails.Text = m_txtDetails.Text & _
                    "Verificiranje datoteke nije uspjelo." & vbCrLf & _
                    "Razlog: " & strResult & vbCrLf & _
                    "" & vbCrLf
            Else
                Let m_txtDetails.Text = m_txtDetails.Text & _
                    "Verificiranje datoteke je uspjelo." & vbCrLf & _
                    "Pohranjena datoteka: " & strResult & vbCrLf & _
                    "" & vbCrLf
                If vaT(LBound(vaT) + 3) Then
                    Let m_txtDetails.Text = m_txtDetails.Text & _
                        "Datoteka je dešifrirana." & vbCrLf
                    Set oCert = vaT(LBound(vaT) + 4)
                    If Not Nothing Is oCert Then
                        GoSub LBL_OUTPUT_CERT
                    End If
                    Let m_txtDetails.Text = m_txtDetails.Text & _
                        "" & vbCrLf
                End If
                If vaT(LBound(vaT) + 5) Then
                    Let m_txtDetails.Text = m_txtDetails.Text & _
                        "Potpis je ovjeren." & vbCrLf & _
                        "" & vbCrLf
                End If
            End If
            
        Case Else
            Let m_txtDetails.Text = ""
        
    End Select

    Exit Sub
LBL_OUTPUT_CERT:
    Let m_txtDetails.Text = m_txtDetails.Text & _
        "Neslužbeni naziv: '" & oCert.FriendlyName & "'" & vbCrLf & _
        "Predmet: '" & oCert.Subject & "'" & vbCrLf & _
        "Izdavaè: '" & oCert.IssuerSerial.IssuerName & "'" & vbCrLf & _
        "Serijski broj: '" & oCert.IssuerSerial.SerialNumber & "'" & vbCrLf & _
        "Otisak prsta: '" & oCert.Thumbprint & "'" & vbCrLf & _
        IIf(oCert.Valid, "Certifikat je valjan.", "Certifikat NIJE valjan.") & vbCrLf
    Return
LBL_FAIL:
    Call MsgBox(Err.Description, vbCritical)
    Let m_txtDetails.Text = ""
End Sub

Private Function SelectCertificate( _
    ByVal nCertLocation As CertificateLocation _
  , ByVal nCertPurpose As CertificatePurpose _
  , ByVal strThumbprint As String _
  , ByVal strTitle As String _
  , ByVal strMessage As String _
  , ByRef oCert As MammSignerLib.Certificate _
  ) As Boolean
    Let SelectCertificate = False
    
    With New CUiHourglass
        Call Certificates.LoadCertificates(nCertLocation, m_oAppOptions.IncludeCsp)
    End With
    If "" <> strThumbprint Then
        ' Certifikat je konfiguriran u programskim opcijama.
        Set oCert = Certificates.FindCertificate(strThumbprint, ValidOnly:=False)
        If Nothing Is oCert Then Call Err.Raise("Certifikat konfiguriran programskim opcijana nije naðen.")
    Else
        ' Korisnik mora izrabrati certifikat.
        Set oCert = Certificates.SelectCertificate(IIf(m_oAppOptions.IgnorePurpose, CertificatePurpose.Unspecified, nCertPurpose), Not m_oAppOptions.AllowInvalid, strTitle, strMessage)
    End If
    Let SelectCertificate = Not Nothing Is oCert
End Function

Private Function UiUpdateState()
    Let m_cmdSign.Enabled = 0 < m_lstFiles.ListCount
    Let m_cmdVerify.Enabled = m_cmdSign.Enabled
    Call m_lstFiles_Click
End Function
