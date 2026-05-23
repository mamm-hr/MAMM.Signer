# MAMM.Signer.Core

Zajednička biblioteka za programe [MAMM.Signer.Cli](../MAMM.Signer.Cli/README.md) i
[MAMM.Signer.Gui](../MAMM.Signer.Gui/README.md).

Središnji razred je [AppOperations](./AppOperations.cs). Sadrži:

- metodu za enumeriranje ulaznih datoteka zadanih uzorkom za traženje (* i ?), 
- metodu za enumeriranje ulaznih datoteka zadanih popisom,
- metodu koja izvršava obradu pojedinačne ulazne datoteke i generira objekt ishoda i 
- metodu koja temeljem objekta ishoda vrši oporavak ako je metoda obrade završila u grešci.

Razred [AppOptions](./AppOptions.cs) sadrži izvršne opcije za program, v. [MAMM.Signer.Cli](../MAMM.Signer.Cli/README.md)
za detaljni opis.

Razred [AppResult](./AppResult.cs) sadrži ishod rada programa, tj. popis ishoda obrada ulaznih datoteka, kao i opisnik
iznimke ako je obrada završila u grešci.

U datoteci [Exceptions.cs](./Exceptions.cs) sadržane su iznimke koje ova biblioteka može baciti.

Sučelje [ICertificateManager](./ICertificateManager) omogućuje metodama ove procedure da dohvate potrebne certifikata, a
da biblioteka ostane separirana od implementacije tog postupka.

---
