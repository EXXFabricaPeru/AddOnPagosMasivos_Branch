CREATE PROCEDURE EXD_SP_PM_H2H_DATOS_TXT_BCP
(
	NroPM int,
	NroSC int,
	NroCT varchar(50)
)
AS
BEGIN
	declare idEnvio int;
	select count('A') into idEnvio from "@EXD_PM_LOGENVHTH" where U_COD_BANCO = '002' and U_FECHA_ENVIO = TO_DATE(NOW());
	with CTE_CAB AS
	(
		select
			'1'															as "TipoRegistro",
			sum(1)														as "CntDeAbonos",
			TO_VARCHAR(NOW(),'yyyyMMdd')								as "FechaProceso",
			'C'															as "TipoCtaCargo",
			case when T1."U_EXP_MONEDA" = 'SOL' then '0001' else '1001' end	as "Moneda",
			rpad(replace(T3."Account",'-',''),20,' ')					as "NroCtaCargo",
			sum(TO_DECIMAL(T1."U_EXP_IMPORTE",16,2))					as "TotalPlanilla",
			ifnull(T1."U_EXP_COMENTARIO",'')							as "Referencia",
			'N'															as "FlagExoITF",
			''															as "TotalControl",
			''															as "Filler",
			''															as "NroPlanilla",
			''															as "Estado"
		from  
		"@EXP_PMP1" 				T1
		inner join "@EXP_OPMP" 		T2 on T1."DocEntry" 	= T2."DocEntry"
		inner join DSC1 			T3 on T3."GLAccount"	= T1."U_EXP_CODCTABANCO" and T1."U_EXP_COD_SUCURSAL" = T3."Branch"
		where T3."BankCode" = '002' 
		and T2."DocEntry" = :NroPM
		and T1."U_EXP_COD_SUCURSAL" = :NroSC
		and T1."U_EXP_CODCTABANCO" = :NroCT
		group by T2."CreateDate",T1."U_EXP_MONEDA",T3."Account",T1."U_EXP_COMENTARIO",T2."DocNum"
	),

	CTE_PROV AS
	(
		select 
			'2'																		as "TipoRegistro",
			case when T3."BankCode" != '002' then 'B' else T4."UsrNumber2" end		as "TipoCuentaAbono",
			case when T3."BankCode" != '002' 
			then 
				left(replace(T4."U_EXM_INTERBANCARIA",'-',''),20) 
			else 
				replace(T4."Account",'-','') end									as "NroCtaAbono",
			'1'																		as "ModalidadDePago",
			ifnull(case T3."U_EXX_TIPODOCU" 
					when '4' then '3'
					when '7' then '4'
					else T3."U_EXX_TIPODOCU" end,'') 								as "TipoDocumentoProv",
			T3."LicTradNum"															as "NroDocProv",
			'   '																	as "CorrDocProv",
			T3."CardName"															as "NombreProveedor",
			ifnull(T1."U_EXP_NROSUNAT",'')											as "ReferenciaProveedor",
			T1."DocEntry"||'-'||T1."LineId"											as "ReferenciaEmpresa",
			case when T1."U_EXP_MONEDA" = 'SOL' then '0001' else '1001' end			as "Moneda",
			TO_DECIMAL(T1."U_EXP_IMPORTE",16,2)										as "Importe",
			'S'																		as "Validar",
			''																		as "Filler"
		--from OVPM T0 
		from "@EXP_PMP1"		T1 
		inner join "@EXP_OPMP"	T2 on T1."DocEntry" 		= T2."DocEntry"
		inner join OCRD			T3 on T1."U_EXP_CARDCODE"	= T3."CardCode"
		inner join OCRB			T4 on T3."CardCode"			= T4."CardCode" and T1."U_EXP_CODBANCOPROV" = T4."BankCode"
		where 
		T2."DocEntry" = :NroPM 
		and T1."U_EXP_COD_SUCURSAL" = :NroSC 
		and T1."U_EXP_CODCTABANCO" =:NroCT
		and ifnull(T4."U_EXC_ACTIVO",'') = 'Y'  
		and T4."UsrNumber1" = "U_EXP_MONEDA"
	)	
	select "Data",'P'||TO_VARCHAR(NOW(),'yyyyMMdd')||lpad(idEnvio+1,6,'0')||'P' as "Nombre" from 
	(
		select 
			"TipoRegistro"			||
			lpad("CntDeAbonos",6,'0')		||
			"FechaProceso"			||
			"TipoCtaCargo"			||
			"Moneda"				||
			rpad("NroCtaCargo",20,' ')			||
			lpad(round("TotalPlanilla",2),17,'0')			||
			rpad(left("Referencia",40),40,' ')			||
			"FlagExoITF"			||
			lpad(to_bigint(right(trim("NroCtaCargo"),10))+(select sum(to_bigint(right(trim("NroCtaAbono"),10))) from CTE_PROV),15,'0')||
			rpad("Filler",100,' ') ||
			lpad('',6,' ') ||
			lpad('',6,' ') ||
			lpad('',6,' ') ||
			lpad("Estado",80,' ')
			||'Z' as "Data"
		from CTE_CAB
		union all 
		select 
			"TipoRegistro"			||
			"TipoCuentaAbono"		||
			lpad(ifnull("NroCtaAbono",''),20,' ') ||
			"ModalidadDePago"		||
			"TipoDocumentoProv"		||
			rpad("NroDocProv",12,' ')			||
			"CorrDocProv"			||
			rpad(left("NombreProveedor",75),75,' ')		||
			rpad(left("ReferenciaProveedor",40),40,' ')	||
			rpad(left("ReferenciaEmpresa",20),20,' ')	||
			"Moneda"				||
			lpad(round("Importe",2),17,'0')				||
			"Validar"||
			lpad("Filler",100,' ')
			||'Z' as "Data"
		from CTE_PROV
	);
	/*
	union all 
	select 
		"TipoRegistro"		||
		"TipoDocumento"		||
		lpad(ifnull("NroDocAPagar",''),15,'0')	||
		lpad("Importe",17,'0')
		||'Z' as "Data"
	from CTE_BENEF;
	*/
END;