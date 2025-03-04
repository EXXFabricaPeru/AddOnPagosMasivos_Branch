CREATE PROCEDURE EXD_SP_PM_H2H_DATOS_TXT_BBVA
(
	NroPM int,
	NroSC int,
	NroCT varchar(50)
)
AS
BEGIN
	
	declare nroRUC varchar(20);
	declare idEnvio int;
	
	select "GlblLocNum" into nroRUC from OBPL where "BPLId" = :NroSC;
	select count('A') into idEnvio from "@EXD_PM_LOGENVHTH" where U_COD_BANCO = '011' and U_FECHA_ENVIO = TO_DATE(NOW());
	
	with CTE_DATOS_CAB AS
	(
		select distinct
			'3110'															as "CodigoRegistro",
			'R'																as "TipoDocOrdenante",
			rpad(T2."GlblLocNum",12,' ')/*T5."TaxIdNum"*/					as "DocumentoOrdenante",
			TO_VARCHAR(NOW(),'ddMMyyyy')									as "FechaCreacion",
			TO_VARCHAR(NOW(),'ddMMyyyy')									as "FechaProceso",	
			T1."Account"													as "CuentaOrdenante",	
			case when T0."U_EXP_MONEDA_PAGO" = 'SOL' then 'PEN' else 'USD' end 	as "Moneda",
			lpad('',12,' ')													as "Filler",	
			'1'																as "ValidacionPertenencia",
			'0'																as "IndicadorDevolucion",
			'BUSI0000H2H_PE_'||TO_VARCHAR(NOW(),'yyyyMMdd')
			|| right('000'||:idEnvio+1,3)									as "NombreFichero",
			rpad('',3,' ')													as "Servicio",
			rpad('',16,' ')													as "InfComplementaria",
			rpad('',146,' ')												as "Filler2"		
		from "@EXP_PMP1" 	T0
		inner join DSC1		T1 on T1."GLAccount"		= T0."U_EXP_CODCTABANCO" and T0."U_EXP_COD_SUCURSAL" = T1."Branch"
		inner join OBPL 	T2 on T2."BPLId"			= T0."U_EXP_COD_SUCURSAL"
		where T0."DocEntry" = :NroPM and T0."U_EXP_COD_SUCURSAL" = :NroSC and T0."U_EXP_CODCTABANCO" = :NroCT
	),
	
	CTE_DATOS_CAB2 as
	(
		select
			'3120'													as "CodigoRegistro",
			'R'														as "TipoDocumento",
			rpad("GlblLocNum",12,' ')/*"TaxIdNum"*/ 				as "NroDocumento",
			rpad("BPLName",35,' ')/*"PrintHeadr"*/					as "Nombre",
			rpad("Address",35,' ')									as "Domicilio",
			rpad('',168,' ')										as "Filler"										
		from OBPL where "BPLId" = :NroSC 
	),

	CTE_DATOS_PROV AS
	(
		select distinct
			'3210'															as "CodigoRegistro",
			'R'																as "TipoDocOrdenante",
			rpad(:nroRUC,12,' ')/*T6."TaxIdNum"*/							as "NroDocOrdenante",
			'R'																as "TipoDocProv",
			left(rpad(T0."U_EXP_NRODOCUMENTOSN",12,' '),12)					as "NroDocProv",
			left(rpad(ifnull(T0."U_EXP_CARDNAME",''),35,' '),35)			as "NombreProv",
			lpad('',12,'0')													as "Importe1",
			'10'															as "Importe2",
			case when T0."U_EXP_MONEDA" = 'SOL' then 'PEN' else 'USD' end	as "Divisa",
			lpad('',12,' ') 												as "Filler",
			'0000'															as "CodigoDevolucion",
			rpad('',40,' ')													as "MensajeDev",
			rpad('',117,' ')												as "Filler2"
		from "@EXP_PMP1"			T0 
		where T0."DocEntry" = :NroPM and T0."U_EXP_COD_SUCURSAL" = :NroSC and T0."U_EXP_CODCTABANCO" = :NroCT
	),
	
	CTE_DATOS_PROV2 AS
	(
		select distinct
			'3220'																as "CodigoRegistro",
			'R'																	as "TipoDocOrdenante",
			rpad(:nroRUC,12,' ')/*T6."TaxIdNum"*/								as "NroDocOrdenante",
			'R'																	as "TipoDocProv",
			left(rpad(T0."U_EXP_NRODOCUMENTOSN",12,' '),12)						as "NroDocProv",
			'0011'																as "CodigoCtaDebitar",
			left(lpad(replace(T0."U_EXP_NROCTAPROV",'-',''),20,'0'),20)			as "NroCtaProv",
			lpad('',35,' ')														as "DomicilioProv",
			case when T0."U_EXP_CODBANCOPROV" = '011' then 'P' else 'I' end		as "FormaDePago",
			lpad('',12,' ')														as "Fax",
			lpad('',12,' ')														as "Telefono",
			case when T0."U_EXP_CODBANCOPROV" = '011' then '00' else '02' end	as "TipoDeCuenta",
			rpad('',139,' ')													as "Filler"
		from "@EXP_PMP1" T0
		where T0."DocEntry" = :NroPM and T0."U_EXP_COD_SUCURSAL" = :NroSC and T0."U_EXP_CODCTABANCO" = :NroCT
	),
	
	CTE_DATOS_FAC_PROV AS
	(
		select distinct
			'3310'															as "CodigoRegistro",
			'R'																as "TipoDocOrdenante",
			rpad(:nroRUC,12,' ')/*T6."TaxIdNum"*/							as "NroDocOrdenante",
			'R'																as "TipoDocProv",
			rpad(T0."U_EXP_NRODOCUMENTOSN",12,' ')							as "NroDocProv",
			'1'																as "TipoDocXP",
			rpad(T0."U_EXP_NROSUNAT",12,' ')								as "NroDocXP",
			TO_VARCHAR(NOW(),'ddMMyyyy')						   	 		as "FechaDocXP",
			TO_VARCHAR(NOW(),'ddMMyyyy')									as "FechaVencDocXP",
			case when T0."U_EXP_MONEDA" = 'SOL' then 'PEN' else 'USD' end	as "DivisaDocXP",
			'000000000000'													as "Importe1",
			'01'															as "Importe2",
			'1'																as "SignoImporte",
			rpad('',25,' ')													as "Concepto",
			rpad('',153,' ')												as "Filler",
			0.01															as "ImporteDocumento"
		from "@EXP_PMP1" T0 			
		where T0."DocEntry" = :NroPM and T0."U_EXP_COD_SUCURSAL" = :NroSC and T0."U_EXP_CODCTABANCO" = :NroCT
	),
	CTE_DATOS_TOTALES AS
	(
		select
			'3910'														as "CodigoRegistro",
			'R'															as "TipoDocumento",
			rpad(:nroRUC,12,' ')/*"TaxIdNum"*/ 					as "NroDocumento",
			rpad('',206,' ')											as "Filler"							
		from OADM	
	)
	select "Data",(select "NombreFichero" from CTE_DATOS_CAB) as "Nombre","Orden1","Orden2","Orden3" from 
	(
		select
			1 "Orden1",
			'1' "Orden2",
			1 "Orden3",
			"CodigoRegistro" ||
			"TipoDocOrdenante" ||
			"DocumentoOrdenante" ||
			"FechaCreacion" ||
			"FechaProceso" ||
			"CuentaOrdenante" ||
			"Moneda" ||
			"Filler" ||
			"ValidacionPertenencia" ||
			"IndicadorDevolucion" ||
			left("NombreFichero",20)	||
			"Servicio" ||
			"InfComplementaria" ||
			"Filler2" || 'X' as "Data"
		from CTE_DATOS_CAB
	
		union all
		
		select
			2,	
			'2',
			2,
			"CodigoRegistro" ||
			"TipoDocumento"||
			"NroDocumento"||
			"Nombre"||
			"Domicilio"||
			"Filler" || 'X'	as "Data"
		from CTE_DATOS_CAB2
	
		union all

		select 
			3,
			"NroDocProv",
			3,
			"CodigoRegistro"||
			"TipoDocOrdenante"||
			"NroDocOrdenante"||
			"TipoDocProv"||
			"NroDocProv"||
			"NombreProv"||
			lpad(floor((select sum(TX0."ImporteDocumento") from CTE_DATOS_FAC_PROV TX0 where TX0."NroDocProv" = T0."NroDocProv")),12,'0')||
			lpad(right((select sum(TX0."ImporteDocumento") from CTE_DATOS_FAC_PROV TX0 where TX0."NroDocProv" = T0."NroDocProv"),2),2,'0')||
			"Divisa"||
			"Filler"||
			"CodigoDevolucion"||
			"MensajeDev"||
			"Filler2" || 'X' as "Data"
		from CTE_DATOS_PROV T0
			
		union all
			
		select
			3,
			"NroDocProv",
			4,
			"CodigoRegistro"||
			"TipoDocOrdenante"||
			"NroDocOrdenante"||
			"TipoDocProv"||
			"NroDocProv"||
			"CodigoCtaDebitar"||
			"NroCtaProv"||
			"DomicilioProv"||
			"FormaDePago"||
			"Fax"||
			"Telefono"||
			"TipoDeCuenta" ||
			"Filler" || 'X' as "Data"
		from CTE_DATOS_PROV2
			
		union all
			
		select
			3,
			"NroDocProv",
			5,
			"CodigoRegistro"||
			"TipoDocOrdenante"||
			"NroDocOrdenante"||
			"TipoDocProv"||
			"NroDocProv"||
			"TipoDocXP"||
			"NroDocXP"||
			"FechaDocXP"||
			"FechaVencDocXP"||
			"DivisaDocXP"||
			"Importe1"||
			"Importe2"||
			"SignoImporte"||
			"Concepto"||
			"Filler" || 'X' as "Data"
		from CTE_DATOS_FAC_PROV
			
		union all
		
		select 
			4,
			'Z',
			6,
			"CodigoRegistro" ||
			"TipoDocumento" ||
			"NroDocumento"||
			lpad(((select count('A') from CTE_DATOS_PROV)*2)+3,10,'0') ||
			lpad((select count('A') from CTE_DATOS_FAC_PROV),8,'0')||
			lpad((select floor(sum("ImporteDocumento")) from CTE_DATOS_FAC_PROV),12,'0')||
			lpad((select (sum("ImporteDocumento")-floor(sum("ImporteDocumento")))*100 from CTE_DATOS_FAC_PROV),2,'0')||
			"Filler" || 'X' as "Data"
		from CTE_DATOS_TOTALES
		
	)order by 3,4,5 asc;

	
	/*
	select 
		"CodigoRegistro" ||
		"Rubro" ||
		"CodEmpresa" ||
		"CodServicio" ||
		"CuentaCargo" ||
		"TipoCuentaCargo" ||
		"MonedaCuentaCargo"||
		"NombreSolicitud"||
		"FechaCreacion"||
		"TipoProceso"||
		"FechaProceso"||
		"NroRegistros"||
		"TotalSoles"||
		"TotalDolares"||
		"VersionMacro" as "Data"		
	from CTE_DATOS_CAB
	
	union all
	
	select	
		"CodigoRegistro"||
		"CodigoBeneficiario"||
		"TipoDocumento"||
		"NroDocumento"||
		"FechaVencimiento"||
		"MonedaAbono"||
		"MontoAbono"||
		"IndicadorBanco"||
		"TipoAbono"||
		"TipoCuenta"||
		"MonedaCuenta"||
		"OficinaCuenta"||
		"NumeroCuenta"||
		"TipoPersona"||
		"TipoDocIdentidad"||
		"NroDocIdentidad"||
		"NombreBenef"||
		"NroCelular"||
		"CorreoElectronico" as "Data"
	from CTE_DATOS_DET;*/
END;