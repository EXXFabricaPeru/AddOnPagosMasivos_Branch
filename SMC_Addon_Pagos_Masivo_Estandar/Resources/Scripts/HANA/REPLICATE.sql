CREATE FUNCTION REPLICATE(IN checkString nvarchar, IN length INT) 
RETURNS replicate nvarchar(5000) 
LANGUAGE SQLSCRIPT AS BEGIN  DECLARE v_index INT; 
DECLARE tmp_string nvarchar(5000) := :checkString;  
	FOR v_index IN 1 .. length-1 
	DO tmp_string := :tmp_string || :checkString; 
	END FOR;  
	replicate := tmp_string;  
END;