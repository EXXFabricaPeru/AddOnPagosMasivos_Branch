﻿CREATE  PROCEDURE "SMC_APM_REG_ESCENARIOCAB"
(	
	CODIGO INT,
	DESCRIPCION VARCHAR(100),
	CTACONT VARCHAR(20),
	TIPO VARCHAR(3),
	FECHA DATE,
	USUARIOCONECTADO VARCHAR(50),
	MEDIO_PAGO NVARCHAR(2),
	ESTADO_ESC NVARCHAR(1)
)
AS
BEGIN

	INSERT INTO 
		"@SMC_APM_ESCCAB"
	VALUES
	(
		:CODIGO,
		:CODIGO,
		:DESCRIPCION,
		:TIPO,
		:CTACONT,
		:FECHA,
		'',
		'',
		'',
		'',
		'',
		'',
		:MEDIO_PAGO,
		:ESTADO_ESC
	);
	
	
	--select * from  "@SMC_APM_AUTORIZAR"
	insert into "@SMC_APM_AUTORIZAR" 
	values (
		CAST(:CODIGO AS VARCHAR),
		CAST(:CODIGO AS VARCHAR),
		:USUARIOCONECTADO,
		:CODIGO,
		'N',
		'N'
		);
	
END;