CREATE PROCEDURE "SMC_APM_REG_ESCENARIOCAB"
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
		"@SMC_APM_ESCCAB"(
	 "Code",
	 "Name",
	 "U_SMC_ESC_DESC",
	 "U_SMC_TIPO",
	 "U_SMC_CTACONT",
	 "U_SMC_FECHA",
	 "U_SMC_TXT3RET",
	 "U_SMC_ESTADO",
	 "U_SMC_RCEMB",
	 "U_SMC_RCLIB",
	 "U_SMC_TXTEMB",
	 "U_SMC_FECHAS",
	 "U_EXD_MEDP",
	 "U_EXD_ESTE")
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
	insert into "@SMC_APM_AUTORIZAR"("Code","Name","U_SMC_USUARIO_CREADOR","U_SMC_COD_ESCENARIO"
	,"U_SMC_ACCION","U_SMC_ACCION2","U_SMC_ESTAT")
	values (
		CAST(:CODIGO AS VARCHAR),
		CAST(:CODIGO AS VARCHAR),
		:USUARIOCONECTADO,
		:CODIGO,
		'N',
		'N',
		'PE'
		);
	
END;