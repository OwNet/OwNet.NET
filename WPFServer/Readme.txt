Ak nejde spustit server vdaka exception v Server.cs, ohladom nemoznosti pouzit port 
a nepustame server ako administrator, tak spustime command prompt s administratorskymi
pravami a napiseme:

netsh http add urlacl url=http://+:57116/ user=<NAZOV_POCITACA>\<POUZIVATELSKE_KONTO>

kde <NAZOV_POCITACA> a <POUZIVATELSKE_KONTO> nahradime za zodpovedajuce udaje.

Plati pre Windows 7, pre XP je cesta zlozitejsia:
http://social.msdn.microsoft.com/Forums/en/wcf/thread/285b4c88-0f8c-40b7-81f9-988fef37bd42
http://www.leastprivilege.com/HttpCfgACLHelper.aspx



Martin