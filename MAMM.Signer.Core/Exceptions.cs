namespace MAMM.Signer.Core;

public class SignerException
    : Exception
{
    public SignerException(
          string message
        , Dictionary<string, object> data
        )
        : base( message )
    {
        foreach(var kv in data)
            this.Data.Add( kv.Key, kv.Value );
    }
}

public class InputSpecArgumentMissingException(
    )
    : SignerException( Resources.InputSpecMissing, [] );

public class TooManyArgumentsException(
      int expected
    , int actual
    )
    : SignerException( Resources.TooManyArguments, new(){ [nameof(expected)] = expected, [nameof(actual)] = actual } );

public class ParameterMissingException(
      string arg
    )
    : SignerException( Resources.ParameterMissing, new() { [nameof( arg )] = arg } );

public class NamelessParameterException(
      string arg
    )
    : SignerException( Resources.NamelessParameter, new() { [nameof( arg )] = arg } );

public class MultipleCertificatesException(
      string thumbprint
    )
    : SignerException( Resources.MultipleCertificates, new() { [nameof( thumbprint )] = thumbprint } );

public class SignCeriticateNotSelectedException(
    )
    : SignerException( Resources.SignCeriticateNotSelected, [] );

public class SignCeriticateNotFoundException(
    )
    : SignerException( Resources.SignCeriticateNotFound, [] );

public class EncryptCeriticateNotSelectedException(
    )
    : SignerException( Resources.EncryptCeriticateNotSelected, [] );

public class EncryptCeriticateNotFoundException(
    )
    : SignerException( Resources.EncryptCeriticateNotFound, [] );

public class InvalidOutputFileExtensionException(
      string ext
    )
    : SignerException( Resources.InvalidOutputFileExtension, new() { [nameof( ext )] = ext } );

public class InputListFileDoesNotExistException(
      string file
    )
    : SignerException( Resources.InputListFileDoesNotExist, new() { [nameof( file )] = file } );

public class InputFileDoesNotExistException(
      string file
    )
    : SignerException( Resources.InputFileDoesNotExist, new() { [nameof( file )] = file } );

public class WrongSignVerificationDataTypeException(
      string oid
    )
    : SignerException( Resources.WrongSignVerificationDataType, new() { [nameof( oid )] = oid } );

