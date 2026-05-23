VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CAppOptions"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
Option Explicit

Public AllowInvalid As Boolean

Public Encrypt As Boolean

Public EncryptAlg As String

Public EncryptCert As String

Public EncryptLoc As CertificateLocation

Public Ext As String

Public IgnorePurpose As Boolean

Public IncludeCsp As Boolean

Public OutDir As String

Public Sign As Boolean

Public SignCert As String

Public SignLoc As CertificateLocation

Public Verify As Boolean

Private Sub Class_Initialize()
    Let Me.Ext = ".p7m"
End Sub
