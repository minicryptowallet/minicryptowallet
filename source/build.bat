
@set PATH=%PATH%;%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
@set FILES=BouncyCastle.cs ETHAddress.cs ETHTransaction.cs EllipticCurve.cs Encode.cs Password.cs /r:BouncyCastle.Crypto.dll /r:System.Numerics.dll /r:System.Xaml.dll /r:WPF\PresentationCore.dll /r:WPF\PresentationFramework.dll /r:WPF\WindowsBase.dll

csc /nologo /target:winexe                      /out:generate_address.exe         generate_address.cs       %FILES% ConfigFile.cs WPF.cs
csc /nologo                                                                       generate_address_batch.cs %FILES%
csc /nologo /target:winexe /define:CURRENCY_ETH /out:ETH_generate_transaction.exe generate_transaction.cs   %FILES% ConfigFile.cs WPF.cs
csc /nologo /target:winexe /define:CURRENCY_ETC /out:ETC_generate_transaction.exe generate_transaction.cs   %FILES% ConfigFile.cs WPF.cs
csc /nologo                                                                       send_transaction.cs
csc /nologo                                                                       tx_example.cs             %FILES%

pause
