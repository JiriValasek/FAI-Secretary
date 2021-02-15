# Scénáře použití

Tajemník katedry potřebuje mít možnost:
* přidat/smazat/(upravit) předmět
	* může vyžádat přegenerování StudentCounts
* přidat/smazat/(upravit) zaměstance
	* může vyžádat úpravy Label.employeeId
* přidat/smazat/(upravit) počty studentů v ročnících, semestrech a typech
  studia jednotlivých programů v daných jazycích
	* může vyžádat přidání/změnu/odebrání Labels
* přidělit a odebrat štítky zaměstancům
	* může upravit Label.employeeId
* zkontrolovat bodové vytížení zaměstnanců 
  (1000 bodů = 100 procent z plného pracovního úvazku)
* zjistit, nepřidělené štítky

Tajemník potřebuje vědět:
* jsou-li některé štítky nepřiřazeny
* překračuje-li zaměstnanecovo bodové vytížení stanovenou horní hranici (500 + delta)
* je-li zaměstnancovo bodové vytížení pod stanovenou dolní hranicí (500 - delta)

## Potřebné doplňující otázky

* Q: Může být více přednášejících/cvičících?
  A: V základní aplikaci tuto možnost neuvažujeme.
* Q: Jak nastavit přepočty na body?
  A: Pomocí XML, do budoucna možná formuláře.
* Q: Uvažujeme i zápočty, zkoušky a klasifikované zápočty?
  A: V základní aplikaci tuto možnost neuvažujeme.
* Q: Jsou v aplikaci započítáni i studenti opakující nějaký předmět?
  A: V základní aplikaci tuto možnost neuvažujeme.
* Q: Je možné sdílet soubory/přístup z více míst?
  A: V základní aplikaci je přístup možný pouze z jednoho počítače.
     Řešením může být přístup ke vzdálené ploše (RDP) přes virtuální privátní síť (VPN).

## Více štítků

* Štítek = jedno číslo v rozsahu tabulky I11:L20 Uvazky_example.xlsx
* Výpočet bodů 
 
# Poznámky

* XML serializace (XMLSerializer) / DB / eval is evil
* Pokud použijeme SQL, pak je potřeba ošetřit SQL injection.
