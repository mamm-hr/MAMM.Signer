VERSION 5.00
Begin VB.Form FOptionsDialog 
   BorderStyle     =   3  'Fixed Dialog
   Caption         =   "Opcije"
   ClientHeight    =   7455
   ClientLeft      =   2760
   ClientTop       =   3750
   ClientWidth     =   12030
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   7455
   ScaleWidth      =   12030
   ShowInTaskbar   =   0   'False
   StartUpPosition =   1  'CenterOwner
   Begin VB.CheckBox m_chkTrustCertificates 
      Caption         =   "Ne provjeravaj lanac povjerenja"
      Height          =   255
      Left            =   7080
      TabIndex        =   38
      Top             =   6840
      Width           =   4695
   End
   Begin VB.Frame m_frmT 
      Caption         =   "Primatelj"
      Height          =   2055
      Index           =   3
      Left            =   240
      TabIndex        =   25
      Top             =   5160
      Width           =   6735
      Begin VB.OptionButton m_optEncryptCertLoc 
         Caption         =   "Osobni certifikati"
         Height          =   255
         Index           =   0
         Left            =   1680
         TabIndex        =   27
         Top             =   360
         Width           =   1815
      End
      Begin VB.OptionButton m_optEncryptCertLoc 
         Caption         =   "Pametne kartice"
         Height          =   255
         Index           =   1
         Left            =   3720
         TabIndex        =   28
         Top             =   360
         Width           =   1815
      End
      Begin VB.CommandButton m_cmdClearCert 
         Caption         =   "X"
         Height          =   375
         Index           =   1
         Left            =   5880
         TabIndex        =   36
         Top             =   1200
         Width           =   375
      End
      Begin VB.CommandButton m_cmdSelectCert 
         Caption         =   "..."
         Height          =   375
         Index           =   1
         Left            =   5880
         TabIndex        =   35
         Top             =   720
         Width           =   375
      End
      Begin VB.TextBox m_txtCertName 
         Height          =   324
         Index           =   1
         Left            =   1680
         Locked          =   -1  'True
         TabIndex        =   30
         Top             =   720
         Width           =   4095
      End
      Begin VB.TextBox m_txtCertIssuer 
         Height          =   324
         Index           =   1
         Left            =   1680
         Locked          =   -1  'True
         TabIndex        =   32
         Top             =   1080
         Width           =   4095
      End
      Begin VB.TextBox m_txtCertSerNo 
         Height          =   324
         Index           =   1
         Left            =   1680
         Locked          =   -1  'True
         TabIndex        =   34
         Top             =   1440
         Width           =   4095
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Lokacija:"
         Height          =   195
         Index           =   20
         Left            =   240
         TabIndex        =   26
         Top             =   390
         Width           =   645
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Serijski broj:"
         Height          =   195
         Index           =   19
         Left            =   240
         TabIndex        =   33
         Top             =   1500
         Width           =   840
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Izdavaè:"
         Height          =   195
         Index           =   18
         Left            =   240
         TabIndex        =   31
         Top             =   1140
         Width           =   615
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Certifikat"
         Height          =   195
         Index           =   17
         Left            =   240
         TabIndex        =   29
         Top             =   780
         Width           =   615
      End
   End
   Begin VB.Frame m_frmT 
      Caption         =   "Potpisnik"
      Height          =   2055
      Index           =   2
      Left            =   240
      TabIndex        =   13
      Top             =   3000
      Width           =   6735
      Begin VB.TextBox m_txtCertSerNo 
         Height          =   324
         Index           =   0
         Left            =   1680
         Locked          =   -1  'True
         TabIndex        =   22
         Top             =   1440
         Width           =   4095
      End
      Begin VB.TextBox m_txtCertIssuer 
         Height          =   324
         Index           =   0
         Left            =   1680
         Locked          =   -1  'True
         TabIndex        =   20
         Top             =   1080
         Width           =   4095
      End
      Begin VB.TextBox m_txtCertName 
         Height          =   324
         Index           =   0
         Left            =   1680
         Locked          =   -1  'True
         TabIndex        =   18
         Top             =   720
         Width           =   4095
      End
      Begin VB.CommandButton m_cmdSelectCert 
         Caption         =   "..."
         Height          =   375
         Index           =   0
         Left            =   5880
         TabIndex        =   23
         Top             =   720
         Width           =   375
      End
      Begin VB.CommandButton m_cmdClearCert 
         Caption         =   "X"
         Height          =   375
         Index           =   0
         Left            =   5880
         TabIndex        =   24
         Top             =   1200
         Width           =   375
      End
      Begin VB.OptionButton m_optSignCertLoc 
         Caption         =   "Pametne kartice"
         Height          =   255
         Index           =   1
         Left            =   3720
         TabIndex        =   16
         Top             =   360
         Width           =   1815
      End
      Begin VB.OptionButton m_optSignCertLoc 
         Caption         =   "Osobni certifikati"
         Height          =   255
         Index           =   0
         Left            =   1680
         TabIndex        =   15
         Top             =   360
         Width           =   1815
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Certifikat"
         Height          =   195
         Index           =   16
         Left            =   240
         TabIndex        =   17
         Top             =   780
         Width           =   615
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Izdavaè:"
         Height          =   195
         Index           =   15
         Left            =   240
         TabIndex        =   19
         Top             =   1140
         Width           =   615
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Serijski broj:"
         Height          =   195
         Index           =   14
         Left            =   240
         TabIndex        =   21
         Top             =   1500
         Width           =   840
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Lokacija:"
         Height          =   195
         Index           =   13
         Left            =   240
         TabIndex        =   14
         Top             =   390
         Width           =   645
      End
   End
   Begin VB.Frame m_frmT 
      Caption         =   "Algoritmi"
      Height          =   3615
      Index           =   0
      Left            =   7080
      TabIndex        =   37
      Top             =   3000
      Width           =   4695
      Begin VB.TextBox m_txtOidName 
         Height          =   324
         Index           =   5
         Left            =   3480
         TabIndex        =   59
         Top             =   3000
         Width           =   975
      End
      Begin VB.TextBox m_txtOidName 
         Height          =   324
         Index           =   4
         Left            =   3480
         TabIndex        =   56
         Top             =   2640
         Width           =   975
      End
      Begin VB.TextBox m_txtOidName 
         Height          =   324
         Index           =   3
         Left            =   3480
         TabIndex        =   53
         Top             =   2280
         Width           =   975
      End
      Begin VB.TextBox m_txtOidName 
         Height          =   324
         Index           =   2
         Left            =   3480
         TabIndex        =   50
         Top             =   1920
         Width           =   975
      End
      Begin VB.TextBox m_txtOidName 
         Height          =   324
         Index           =   1
         Left            =   3480
         TabIndex        =   47
         Top             =   1560
         Width           =   975
      End
      Begin VB.TextBox m_txtOidName 
         Height          =   324
         Index           =   0
         Left            =   3480
         TabIndex        =   43
         Top             =   720
         Width           =   975
      End
      Begin VB.TextBox m_txtOidValue 
         Height          =   324
         Index           =   5
         Left            =   1680
         TabIndex        =   58
         Top             =   3000
         Width           =   1695
      End
      Begin VB.TextBox m_txtOidValue 
         Height          =   324
         Index           =   4
         Left            =   1680
         TabIndex        =   55
         Top             =   2640
         Width           =   1695
      End
      Begin VB.TextBox m_txtOidValue 
         Height          =   324
         Index           =   3
         Left            =   1680
         TabIndex        =   52
         Top             =   2280
         Width           =   1695
      End
      Begin VB.TextBox m_txtOidValue 
         Height          =   324
         Index           =   2
         Left            =   1680
         TabIndex        =   49
         Top             =   1920
         Width           =   1695
      End
      Begin VB.TextBox m_txtOidValue 
         Height          =   324
         Index           =   1
         Left            =   1680
         TabIndex        =   46
         Top             =   1560
         Width           =   1695
      End
      Begin VB.TextBox m_txtOidValue 
         Height          =   324
         Index           =   0
         Left            =   1680
         TabIndex        =   42
         Top             =   720
         Width           =   1695
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Digitalni sažetak:"
         Height          =   195
         Index           =   12
         Left            =   240
         TabIndex        =   44
         Top             =   1248
         Width           =   1200
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Naziv"
         Height          =   195
         Index           =   11
         Left            =   3480
         TabIndex        =   40
         Top             =   360
         Width           =   405
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "ECDSA P-521"
         Height          =   195
         Index           =   10
         Left            =   240
         TabIndex        =   57
         Top             =   3060
         Width           =   1005
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "ECDSA P-384"
         Height          =   195
         Index           =   9
         Left            =   240
         TabIndex        =   54
         Top             =   2700
         Width           =   1005
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "ECDSA P-256"
         Height          =   195
         Index           =   8
         Left            =   240
         TabIndex        =   51
         Top             =   2340
         Width           =   1005
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "RSA KSP"
         Height          =   195
         Index           =   7
         Left            =   240
         TabIndex        =   48
         Top             =   1980
         Width           =   690
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "RSA CSP"
         Height          =   195
         Index           =   6
         Left            =   240
         TabIndex        =   45
         Top             =   1620
         Width           =   690
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Šifriranje:"
         Height          =   195
         Index           =   5
         Left            =   240
         TabIndex        =   41
         Top             =   780
         Width           =   645
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "OID"
         Height          =   195
         Index           =   4
         Left            =   1680
         TabIndex        =   39
         Top             =   360
         Width           =   285
      End
   End
   Begin VB.CommandButton m_cmdResetOutputDir 
      Caption         =   "X"
      Height          =   375
      Left            =   6600
      TabIndex        =   12
      Top             =   2370
      Width           =   375
   End
   Begin VB.CommandButton m_cmdBrowseForOutputDir 
      Caption         =   "..."
      Height          =   375
      Left            =   6120
      TabIndex        =   11
      Top             =   2370
      Width           =   375
   End
   Begin VB.TextBox m_txtOutputDir 
      Height          =   324
      Left            =   1920
      Locked          =   -1  'True
      TabIndex        =   10
      Top             =   2400
      Width           =   4095
   End
   Begin VB.TextBox m_txtPkcsExt 
      Height          =   324
      Left            =   1920
      TabIndex        =   8
      Top             =   2040
      Width           =   735
   End
   Begin VB.CheckBox m_chkAllowInvalid 
      Caption         =   "Nevaljane certifikate"
      Height          =   255
      Left            =   3600
      TabIndex        =   5
      Top             =   960
      Width           =   3135
   End
   Begin VB.Frame m_frmT 
      Caption         =   "Izabiranje certifikata"
      Height          =   1575
      Index           =   1
      Left            =   240
      TabIndex        =   0
      Top             =   240
      Width           =   6735
      Begin VB.CheckBox m_chkIncludeCsp 
         Caption         =   "Pametne kartice dostupne kroz CSP"
         Height          =   195
         Left            =   3360
         TabIndex        =   6
         Top             =   1080
         Width           =   3135
      End
      Begin VB.OptionButton m_optCertPurpose 
         Caption         =   "Prema kriptografskoj namjeni"
         Height          =   195
         Index           =   1
         Left            =   240
         TabIndex        =   3
         TabStop         =   0   'False
         Top             =   1080
         Width           =   2775
      End
      Begin VB.OptionButton m_optCertPurpose 
         Caption         =   "Sve certifikate"
         Height          =   195
         Index           =   0
         Left            =   240
         TabIndex        =   2
         Top             =   720
         Width           =   2775
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Ukljuèi i:"
         Height          =   195
         Index           =   3
         Left            =   3120
         TabIndex        =   4
         Top             =   360
         Width           =   600
      End
      Begin VB.Label m_lblT 
         AutoSize        =   -1  'True
         Caption         =   "Pokaži:"
         Height          =   195
         Index           =   2
         Left            =   120
         TabIndex        =   1
         Top             =   360
         Width           =   525
      End
   End
   Begin VB.CommandButton m_cmdCancel 
      Cancel          =   -1  'True
      Caption         =   "Odustani"
      CausesValidation=   0   'False
      Height          =   375
      Left            =   10560
      TabIndex        =   61
      Top             =   720
      Width           =   1215
   End
   Begin VB.CommandButton m_cmdOK 
      Caption         =   "OK"
      Height          =   375
      Left            =   10560
      TabIndex        =   60
      Top             =   240
      Width           =   1215
   End
   Begin VB.Label m_lblT 
      AutoSize        =   -1  'True
      Caption         =   "Izlazna mapa:"
      Height          =   195
      Index           =   1
      Left            =   240
      TabIndex        =   9
      Top             =   2460
      Width           =   975
   End
   Begin VB.Label m_lblT 
      AutoSize        =   -1  'True
      Caption         =   "PKCS #7 ekstenzija:"
      Height          =   195
      Index           =   0
      Left            =   240
      TabIndex        =   7
      Top             =   2100
      Width           =   1455
   End
End
Attribute VB_Name = "FOptionsDialog"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Private Const IDX_CERT_FIRST As Integer = 0
Private Const IDX_CERT_SIGN As Integer = IDX_CERT_FIRST + 0
Private Const IDX_CERT_ENCRYPT As Integer = IDX_CERT_FIRST + 1

Private Const IDX_OID_FIRST As Integer = 0
Private Const IDX_OID_ENCRYPT  As Integer = IDX_OID_FIRST + 0
Private Const IDX_OID_RSACSP   As Integer = IDX_OID_FIRST + 1
Private Const IDX_OID_RSAKSP   As Integer = IDX_OID_FIRST + 2
Private Const IDX_OID_ECDSA256 As Integer = IDX_OID_FIRST + 3
Private Const IDX_OID_ECDSA384 As Integer = IDX_OID_FIRST + 4
Private Const IDX_OID_ECDSA521 As Integer = IDX_OID_FIRST + 5

Private Enum UiCertPurposeConstants
    ShowAllCerts
    ShowByPurpose
End Enum

Private m_oAppOptions As CAppOptions
Private m_oPkcs7Options As Pkcs7Options

Public Property Get AppOptions() As CAppOptions
    Set AppOptions = m_oAppOptions
End Property

Public Property Set AppOptions(ByRef newVal As CAppOptions)
    Set m_oAppOptions = newVal
End Property

Public Property Get Pkcs7Options() As Pkcs7Options
    Set Pkcs7Options = m_oPkcs7Options
End Property

Public Property Set Pkcs7Options(ByRef newVal As Pkcs7Options)
    Set m_oPkcs7Options = newVal
End Property

Friend Sub ShowModal( _
    ByVal frmOwner As Form _
  )
    Call UiWriteData
    Call Me.Show(vbModal, frmOwner)
End Sub
  
Private Sub Form_Load()
    Call UiInitOptions(m_optCertPurpose, Array( _
        ShowAllCerts _
      , ShowByPurpose _
      ))
    Call UiInitOptions(m_optSignCertLoc, Array( _
        CertificateLocation.CurrentUser _
      , CertificateLocation.SmartCardReaders _
      ))
    Call UiInitOptions(m_optEncryptCertLoc, Array( _
        CertificateLocation.CurrentUser _
      , CertificateLocation.SmartCardReaders _
      ))
End Sub

Private Sub m_cmdCancel_Click()
    Call Unload(Me)
End Sub

Private Sub m_cmdClearCert_Click(Index As Integer)
    Call UiWriteCert(Index - IDX_CERT_FIRST, Nothing)
End Sub

Private Sub m_cmdOK_Click()
    On Error Resume Next
    Call UiReadData
    If 0 <> Err.Number Then
        Call MsgBox(Err.Description, vbCritical)
        Exit Sub
    End If
    On Error GoTo 0
    Call Unload(Me)
End Sub

Private Sub m_cmdBrowseForOutputDir_Click()
    Dim strT As String
    If Not ShowBrowseForFolder(Me, strT) Then Exit Sub
    Let m_txtOutputDir.Text = strT
End Sub

Private Sub m_cmdResetOutputDir_Click()
    Let m_txtOutputDir.Text = ""
End Sub

Private Sub m_cmdSelectCert_Click(Index As Integer)
    Dim oCert As Certificate
    Select Case Index - IDX_CERT_FIRST
        Case Is = IDX_CERT_SIGN
            With New CUiHourglass
                Call Certificates.LoadCertificates(UiReadOption(m_optSignCertLoc, m_oAppOptions.SignLoc), UiReadCheckBox(m_chkIncludeCsp))
            End With
            Set oCert = Certificates.SelectCertificate( _
                UiReadCertPurpose(m_oAppOptions.IgnorePurpose, CertificatePurpose.Signature) _
              , Not UiReadCheckBox(m_chkAllowInvalid) _
              , "Potpisni certifikat" _
              , "Izaberite certifikat za potpisivanje datoteka." _
              )
        Case Is = IDX_CERT_ENCRYPT
            With New CUiHourglass
                Call Certificates.LoadCertificates(UiReadOption(m_optEncryptCertLoc, m_oAppOptions.EncryptLoc), UiReadCheckBox(m_chkIncludeCsp))
            End With
            Set oCert = Certificates.SelectCertificate( _
                UiReadCertPurpose(m_oAppOptions.IgnorePurpose, CertificatePurpose.Identification) _
              , Not UiReadCheckBox(m_chkAllowInvalid) _
              , "Certifikat primatelja" _
              , "Izaberite certifikat za šifriranje i dešifriranje datoteka." _
              )
        Case Else
            Exit Sub
    End Select
    If Not Nothing Is oCert Then Call UiWriteCert(Index - IDX_CERT_FIRST, oCert)
End Sub

Private Sub m_txtOidName_Validate(Index As Integer, Cancel As Boolean)
    Dim nOidIdx As Integer: nOidIdx = Index - m_txtOidName.LBound
    Dim strName As String: Let strName = m_txtOidName(Index).Text
    If "" = strName Then
        Call UiWriteOid(nOidIdx, "")
    Else
        Dim strOid As String
        On Error Resume Next
        Let strOid = Oids.GetOid(strName)
        Dim bInvalidInput As Boolean
        Let bInvalidInput = 0 <> Err.Number
        On Error GoTo 0
        If Not bInvalidInput Then Let bInvalidInput = Not IsOidInGroup(strOid, nOidIdx)
        If bInvalidInput Then
            Let m_txtOidName(Index).SelStart = 0
            Let m_txtOidName(Index).SelLength = Len(strName)
            Let Cancel = True
        Else
            Call UiWriteOid(nOidIdx, strOid)
        End If
    End If
End Sub

Private Sub m_txtOidValue_Validate(Index As Integer, Cancel As Boolean)
    Dim nOidIdx As Integer: nOidIdx = Index - m_txtOidName.LBound
    Dim strOid As String: Let strOid = m_txtOidValue(Index).Text
    If "" = strOid Then
        Call UiWriteOid(nOidIdx, "")
    Else
        On Error Resume Next
        Dim strName As String
        Let strName = Oids.GetOidName(strOid)
        Dim bInvalidInput As Boolean
        On Error GoTo 0
        If Not bInvalidInput Then Let bInvalidInput = Not IsOidInGroup(strOid, nOidIdx)
        If bInvalidInput Then
            Let m_txtOidValue(Index).SelStart = 0
            Let m_txtOidValue(Index).SelLength = Len(strOid)
            Let Cancel = True
        Else
            Call UiWriteOid(nOidIdx, strOid)
        End If
    End If
End Sub

Private Sub m_txtPkcsExt_Validate(Cancel As Boolean)
    Dim strExt As String: Let strExt = m_txtPkcsExt.Text
    With New CFile
        If "" = strExt Or "." <> Left(strExt, 1) Or Not .IsValidName(m_txtPkcsExt.Text) Then
            Let m_txtPkcsExt.SelStart = 0
            Let m_txtPkcsExt.SelLength = Len(m_txtPkcsExt.Text)
            Let Cancel = True
        End If
    End With
End Sub

Private Function FindCert( _
    ByVal nCertLoc As CertificateLocation _
  , ByVal bIncludeCsp As String _
  , ByVal strThumbprint As String _
  ) As Certificate
    If "" = strThumbprint Then Exit Function
    With New CUiHourglass
        Call Certificates.LoadCertificates(nCertLoc, bIncludeCsp)
        Set FindCert = Certificates.FindCertificate(strThumbprint, ValidOnly:=False)
    End With
End Function

Private Function IsOidInGroup( _
    ByVal strOid As String _
  , ByVal nOidIdx As Integer _
  )
    If IDX_OID_ENCRYPT = nOidIdx Then
        Let IsOidInGroup = Oids.IsEncryptionAlgorithm(strOid)
    Else
        Let IsOidInGroup = Oids.IsHashAlgorithm(strOid)
    End If
End Function

Private Sub UiInitOptions( _
    ByVal aoptUi As Variant _
  , ByVal anTag As Variant _
  )
    Dim i As Integer
    For i = LBound(anTag) To UBound(anTag)
        Let aoptUi(aoptUi.LBound + i - LBound(anTag)).Tag = anTag(i)
    Next i
End Sub

Private Function UiReadCert( _
    ByVal nCertIdx _
  ) As String
    Let UiReadCert = m_txtCertName(m_txtCertName.LBound + nCertIdx).Tag
End Function
  
Private Function UiReadCertPurpose( _
    ByVal bIngorePurpose As Boolean _
  , ByVal nPurpose As CertificatePurpose _
  ) As CertificatePurpose
    Let UiReadCertPurpose = IIf(ShowAllCerts = UiReadOption(m_optCertPurpose, m_oAppOptions.IgnorePurpose), CertificatePurpose.Unspecified, nPurpose)
End Function

Private Function UiReadCheckBox( _
    ByVal chkUi As CheckBox _
  ) As Boolean
    Let UiReadCheckBox = vbChecked = chkUi.Value
End Function

Private Sub UiReadData()
    
    With m_oAppOptions
        
        Let .IgnorePurpose = ShowAllCerts = UiReadOption(m_optCertPurpose, .IgnorePurpose)
        Let .AllowInvalid = UiReadCheckBox(m_chkAllowInvalid)
        Let .IncludeCsp = UiReadCheckBox(m_chkIncludeCsp)
        Let .Ext = m_txtPkcsExt.Text
        Let .OutDir = m_txtOutputDir.Text
        
        Let .SignLoc = UiReadOption(m_optSignCertLoc, .SignLoc)
        Let .SignCert = UiReadCert(IDX_CERT_SIGN)
                
        Let .EncryptLoc = UiReadOption(m_optEncryptCertLoc, .EncryptLoc)
        Let .EncryptCert = UiReadCert(IDX_CERT_ENCRYPT)
    
        Let .EncryptAlg = UiReadOid(IDX_OID_ENCRYPT)
        
    End With
    
    With m_oPkcs7Options
        With .DefaultDigestAlgorithms
            Let .RsaCspOid = UiReadOid(IDX_OID_RSACSP)
            Let .RsaKspOid = UiReadOid(IDX_OID_RSAKSP)
            Let .Ecdsa256Oid = UiReadOid(IDX_OID_ECDSA256)
            Let .Ecdsa384Oid = UiReadOid(IDX_OID_ECDSA384)
            Let .Ecdsa521Oid = UiReadOid(IDX_OID_ECDSA521)
        End With
        Let .TrustCertificates = UiReadCheckBox(m_chkTrustCertificates)
    End With
    
End Sub

Private Function UiReadOid( _
    ByVal nOidIdx As Integer _
  ) As String
    Let UiReadOid = m_txtOidValue(m_txtOidValue.LBound + nOidIdx).Text
End Function

Private Function UiReadOption( _
    ByVal aoptUi As Variant _
  , ByVal nDefaultTag As Integer _
  ) As Integer
    Let UiReadOption = nDefaultTag
    Dim i As Integer
    For i = aoptUi.LBound To aoptUi.UBound
        If aoptUi(i).Value Then
            Let UiReadOption = aoptUi(i).Tag
            Exit Function
        End If
    Next i
End Function

Private Sub UiWriteCert( _
    ByVal nCertIdx As Integer _
  , ByVal oCert As Certificate _
  )
    If oCert Is Nothing Then
        Let m_txtCertName(m_txtCertName.LBound + nCertIdx).Tag = ""
        Let m_txtCertName(m_txtCertName.LBound + nCertIdx).Text = ""
        Let m_txtCertIssuer(m_txtCertIssuer.LBound + nCertIdx).Text = ""
        Let m_txtCertSerNo(m_txtCertSerNo.LBound + nCertIdx).Text = ""
    Else
        With oCert
            Let m_txtCertName(m_txtCertName.LBound + nCertIdx).Tag = .Thumbprint
            Let m_txtCertName(m_txtCertName.LBound + nCertIdx).Text = .FriendlyOrSubjectName
            Let m_txtCertIssuer(m_txtCertIssuer.LBound + nCertIdx).Text = .IssuerSerial.IssuerName
            Let m_txtCertSerNo(m_txtCertSerNo.LBound + nCertIdx).Text = .IssuerSerial.SerialNumber
        End With
    End If
End Sub

Private Sub UiWriteCheckBox( _
    ByVal chkUi As CheckBox _
  , ByVal bChecked As Boolean _
  )
    Let chkUi.Value = IIf(bChecked, vbChecked, vbUnchecked)
End Sub

Private Sub UiWriteData()
    
    With m_oAppOptions
    
        Call UiWriteOption(m_optCertPurpose, IIf(.IgnorePurpose, ShowAllCerts, ShowByPurpose))
        Call UiWriteCheckBox(m_chkAllowInvalid, .AllowInvalid)
        Call UiWriteCheckBox(m_chkIncludeCsp, .IncludeCsp)
        Let m_txtPkcsExt.Text = .Ext
        Let m_txtOutputDir.Text = .OutDir
        
        Call UiWriteOption(m_optSignCertLoc, .SignLoc)
        Call UiWriteCert(IDX_CERT_SIGN, FindCert(.SignLoc, .IncludeCsp, .SignCert))
                
        Call UiWriteOption(m_optEncryptCertLoc, .EncryptLoc)
        Call UiWriteCert(IDX_CERT_ENCRYPT, FindCert(.EncryptLoc, .IncludeCsp, .EncryptCert))
    
        Call UiWriteOid(IDX_OID_ENCRYPT, .EncryptAlg)
        
    End With
    
    With m_oPkcs7Options
        With .DefaultDigestAlgorithms
            Call UiWriteOid(IDX_OID_RSACSP, .RsaCspOid)
            Call UiWriteOid(IDX_OID_RSAKSP, .RsaKspOid)
            Call UiWriteOid(IDX_OID_ECDSA256, .Ecdsa256Oid)
            Call UiWriteOid(IDX_OID_ECDSA384, .Ecdsa384Oid)
            Call UiWriteOid(IDX_OID_ECDSA521, .Ecdsa521Oid)
        End With
        Call UiWriteCheckBox(m_chkTrustCertificates, .TrustCertificates)
    End With
    
End Sub

Private Sub UiWriteOid( _
    ByVal nOidIdx As Integer _
  , ByVal strOid As String _
  )
    If "" = strOid Then
        Let m_txtOidValue(m_txtOidValue.LBound + nOidIdx).Text = ""
        Let m_txtOidName(m_txtOidName.LBound + nOidIdx).Text = ""
    Else
        Let m_txtOidValue(m_txtOidValue.LBound + nOidIdx).Text = strOid
        Let m_txtOidName(m_txtOidName.LBound + nOidIdx).Text = MammSignerLib.Oids.GetOidName(strOid)
    End If
End Sub

Private Sub UiWriteOption( _
    ByVal aoptUi As Variant _
  , ByVal nTag As Integer _
  )
    Dim i As Integer
    For i = aoptUi.LBound To aoptUi.UBound
        Let aoptUi(i).Value = aoptUi(i).Tag = nTag
    Next i
End Sub

