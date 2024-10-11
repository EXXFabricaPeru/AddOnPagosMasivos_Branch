CREATE PROCEDURE EXD_SP_PM_H2H_DATOS_TXT_INTERBANK
(
	NroPM int,
	NroSC int,
	NroCT varchar(50)
)
AS
BEGIN
	declare idEnvio int;
	declare codEmpresa varchar(4);
	declare fechaCreacion varchar(14);
	select count('A') into idEnvio from "@EXD_PM_LOGENVHTH" where U_COD_BANCO = '003' and U_FECHA_ENVIO = TO_DATE(NOW());
	select U_COD_EMPRESA into codEmpresa from "@EXD_PM_CNFUSUH2H" where U_COD_BANCO = '003' and U_COD_SUCURSAL = NroSC;
	fechaCreacion := TO_VARCHAR(now(),'yyyyMMddHHmmss');
	
	with CTE_DATOS_CAB AS
	(
		select top 1
			'01'																as "CodigoRegistro",
			'03'																as "Rubro",
			:codEmpresa															as "CodEmpresa",
			'01'																as "CodServicio",
			left(replace(T1."Account",'-',''),13)								as "CuentaCargo",
			case T1."UsrNumber2" when 'C' then '001' 
			when 'A' then '002' else '   ' end									as "TipoCuentaCargo",
			case T1."UsrNumber4" when 'SOL' then '01' 
			when 'USD' then '10' else '  ' end									as "MonedaCuentaCargo",
			rpad(left(ifnull(T0."U_EXP_COMENTARIO",'PAG PROV IBK'),12),12)		as "NombreSolicitud",
			:fechaCreacion														as "FechaCreacion",
			'0'																	as "TipoProceso",
			TO_VARCHAR(now(),'yyyyMMdd')										as "FechaProceso",
			0																	as "NroRegistros",
			0																	as "TotalSoles",
			0																	as "TotalDolares",
			'MC001'																as "VersionMacro"											
		from "@EXP_PMP1" 	T0
		inner join DSC1		T1 on T1."GLAccount" = T0."U_EXP_CODCTABANCO" and T0."U_EXP_COD_SUCURSAL" = T1."Branch"
		where T0."DocEntry" = :NroPM and T0."U_EXP_COD_SUCURSAL" = :NroSC and T0."U_EXP_CODCTABANCO" = :NroCT
	),

	CTE_DATOS_DET AS
	(
		select 
			'02'																		as "CodigoRegistro",
			rpad(T0."U_EXP_NRODOCUMENTOSN",20,' ')										as "CodigoBeneficiario",
			'F'																			as "TipoDocumento",
			rpad(T0."U_EXP_NROSUNAT",20,' ')											as "NroDocumento",
			TO_VARCHAR(now(),'yyyyMMdd')												as "FechaVencimiento",
			case T0."U_EXP_MONEDA_PAGO" when 'SOL' 
			then '01' when 'USD' then '10' else '  ' end								as "MonedaAbono",
			T0."U_EXP_IMPORTE",
			lpad(floor(T0."U_EXP_IMPORTE"),13,'0')
			|| right('0' || mod(round(T0."U_EXP_IMPORTE",2),1) * 100,2)					as "MontoAbono",
			'0'																			as "IndicadorBanco",
			case when T0."U_EXP_CODBANCOPROV" = '003' then '09' else '99' end			as "TipoAbono",
			case T2."UsrNumber2" when 'C' then '001' when 'A' then '002' else '   ' end	as "TipoCuenta",
			case T0."U_EXP_MONEDA" when 'SOL' then '01' 
			when 'USD' then '10' else '  ' end											as "MonedaCuenta",
			case when T0."U_EXP_CODBANCOPROV" = '003' then '' else '000' end			as "OficinaCuenta",
			case when T0."U_EXP_CODBANCOPROV" = '003' then 
				rpad(left(replace(T0."U_EXP_NROCTAPROV",'-',''),13),23,' ')
			else 
				rpad(left(replace(T0."U_EXP_NROCTAPROV",'-',''),20),20,' ')
			end 																		as "NumeroCuenta",
			'C'																			as "TipoPersona",
			case T1."U_EXX_TIPODOCU" when '1' then '01'	when '6' then '02' 
			when '4' then '03' when '7' then '05' end 									as "TipoDocIdentidad",
			rpad(T1."LicTradNum",15,' ')												as "NroDocIdentidad",
			left(rpad(replace(T1."CardName",',',';'),60,' '),60)						as "NombreBenef",
			'  '																		as "MonedaCTS",
			rpad('',15,' ')																as "MontoCTS",
			rpad('',6,' ')																as "Filler",
			rpad(ifnull(T1."Cellular",''),40,' ')										as "NroCelular",
			rpad(ifnull(T1."E_Mail",''),140,' ' )										as "CorreoElectronico",			
			T0."U_EXP_IMPORTE"															as "ImportePago",
			T0."U_EXP_MONEDA_PAGO"														as "MonedaPago"
		from "@EXP_PMP1" 	T0
		inner join OCRD 	T1 on T0."U_EXP_CARDCODE" 	= T1."CardCode"
		inner join OCRB		T2 on T1."CardCode"			= T2."CardCode" and T0."U_EXP_CODBANCOPROV" = T2."BankCode"
		where T0."DocEntry" = :NroPM and T0."U_EXP_COD_SUCURSAL" = :NroSC and T0."U_EXP_CODCTABANCO" = :NroCT
		and ifnull(T2."U_EXC_ACTIVO",'') = 'Y'  and T2."UsrNumber1" = T0."U_EXP_MONEDA"
	)
	
	
	select "Data",'H2HH00003'||:codEmpresa||'01'||lpad(idEnvio+1,25,'0')||'_'||:fechaCreacion as "Nombre" from 
	(
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
			lpad((select count('A') from CTE_DATOS_DET),6,'0')||
			lpad(ifnull((select floor(sum("ImportePago"))|| right('0' || mod(round(sum("ImportePago"),2),1) * 100,2) from CTE_DATOS_DET where "MonedaPago" = 'SOL'),''),15,'0')||
			lpad(ifnull((select floor(sum("ImportePago"))|| right('0' || mod(round(sum("ImportePago"),2),1) * 100,2) from CTE_DATOS_DET where "MonedaPago" = 'USD'),''),15,'0')||
			"VersionMacro" ||
			'Z' as "Data",1 as "Orden"		
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
		 	"MonedaCTS"||
			"MontoCTS"||
			"Filler"||
			"NroCelular"||
			"CorreoElectronico"||
			'Z' as "Data",2 as "Orden"	
		from CTE_DATOS_DET
		order by 2
	);
END;