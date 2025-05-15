CREATE FUNCTION LIMPIA_CADENA(IN cadena nvarchar(5000)) 
RETURNS 
cadenaLimpia nvarchar(5000) 
LANGUAGE SQLSCRIPT 
AS 
BEGIN 
	cadenaLimpia := replace(:cadena,',',''); 
	cadenaLimpia := replace(:cadenaLimpia,'.',''); 
	cadenaLimpia := replace(:cadenaLimpia,'-',''); 
	cadenaLimpia := replace(:cadenaLimpia,'&',''); 
	cadenaLimpia := replace(:cadenaLimpia,'Ü','U'); 
	cadenaLimpia := replace(:cadenaLimpia,'ü','u'); 
	cadenaLimpia := replace(:cadenaLimpia,'Ñ','N'); 
	cadenaLimpia := replace(:cadenaLimpia,'ñ','n'); 
	cadenaLimpia := replace(:cadenaLimpia,'á','a'); 
	cadenaLimpia := replace(:cadenaLimpia,'é','e'); 
	cadenaLimpia := replace(:cadenaLimpia,'í','i'); 
	cadenaLimpia := replace(:cadenaLimpia,'ó','o'); 
	cadenaLimpia := replace(:cadenaLimpia,'ú','u'); 
	cadenaLimpia := replace(:cadenaLimpia,'Á','A'); 
	cadenaLimpia := replace(:cadenaLimpia,'É','E'); 
	cadenaLimpia := replace(:cadenaLimpia,'Í','I'); 
	cadenaLimpia := replace(:cadenaLimpia,'Ó','O'); 
	cadenaLimpia := replace(:cadenaLimpia,'Ú','U'); 
	cadenaLimpia := rtrim(ltrim(:cadenaLimpia)); 
END;