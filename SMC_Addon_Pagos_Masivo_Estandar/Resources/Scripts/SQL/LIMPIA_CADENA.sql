CREATE FUNCTION LIMPIA_CADENA( @cadena varchar(5000))
RETURNS  varchar(5000)
AS
BEGIN
	declare @cadenaLimpia varchar(5000)
	
	set @cadenaLimpia = replace(@cadena,',','');
	set @cadenaLimpia = replace(@cadenaLimpia,'.','');
	set @cadenaLimpia = replace(@cadenaLimpia,'-','');
	set @cadenaLimpia = replace(@cadenaLimpia,'&','');
	set @cadenaLimpia = replace(@cadenaLimpia,'Ü','U');
  	set @cadenaLimpia = replace(@cadenaLimpia,'ü','u');
  	set @cadenaLimpia = replace(@cadenaLimpia,'Ñ','N');
  	set @cadenaLimpia = replace(@cadenaLimpia,'ñ','n');
  	set @cadenaLimpia = replace(@cadenaLimpia,'á','a');
  	set @cadenaLimpia = replace(@cadenaLimpia,'é','e');
  	set @cadenaLimpia = replace(@cadenaLimpia,'í','i');
  	set @cadenaLimpia = replace(@cadenaLimpia,'ó','o');
  	set @cadenaLimpia = replace(@cadenaLimpia,'ú','u');
  	set @cadenaLimpia = replace(@cadenaLimpia,'Á','A');
  	set @cadenaLimpia = replace(@cadenaLimpia,'É','E');
  	set @cadenaLimpia = replace(@cadenaLimpia,'Í','I');
  	set @cadenaLimpia = replace(@cadenaLimpia,'Ó','O');
  	set @cadenaLimpia = replace(@cadenaLimpia,'Ú','U');
  	set @cadenaLimpia = rtrim(ltrim(@cadenaLimpia));

	return @cadenaLimpia
END