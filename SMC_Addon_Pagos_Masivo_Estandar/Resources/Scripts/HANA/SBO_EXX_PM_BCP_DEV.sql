CREATE PROCEDURE SBO_EXX_PM_BCP_DEV
(
	NroPM int,
	NroSC int,
	NroCT varchar(50)
)
AS
BEGIN
	with CTE_CAB AS
	(
		select
			top 1
			'1'															as "TipoRegistro",
			sum(1)														as "CntDeAbonos",
			TO_VARCHAR(NOW(),'yyyyMMdd')								as "FechaProceso",
			'C'															as "TipoCtaCargo",
			case when T1."U_EXP_MONEDA" = 'SOL' then '0001' else '1001' end	as "Moneda",
			rpad(replace(T3."Account",'-',''),20,' ')					as "NroCtaCargo",
			sum(TO_DECIMAL(T1."U_EXP_IMPORTE",14,2))					as "TotalPlanilla",
			ifnull(T1."U_EXP_COMENTARIO",'')							as "Referencia",
			'N'															as "FlagExoITF",
			''															as "TotalControl",
			''															as "Filler",
			T2."DocNum"													as "NroPlanilla",
			''															as "Estado"
		from  
		"@EXP_PMP1" 				T1
		inner join "@EXP_OPMP" 		T2 on T1."DocEntry" = T2."DocEntry"
		inner join DSC1 			T3 on T3."BankCode" = T1."U_EXP_CODBANCO" and ifnull(T1."U_EXP_COD_SUCURSAL",'0') = ifnull(T3."Branch",'0') and T1."U_EXP_CODCTABANCO" =  T3."GLAccount"
		where T3."BankCode" = '002' --and ifnull(T3."UsrNumber2",'') = 'C'
		and T2."DocEntry" = :NroPM
		and T1."U_EXP_COD_SUCURSAL" = :NroSC
		and T1."U_EXP_CODCTABANCO" = :NroCT
		and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
		group by T2."DocEntry",T1."U_EXP_COD_SUCURSAL",T2."CreateDate",T1."U_EXP_MONEDA",T3."Account",T1."U_EXP_COMENTARIO",T2."DocNum"
	),

	CTE_PROV AS
	(
		select 
			'2'																			as "TipoRegistro",
			case T1."U_EXP_CODBANCOPROV" when '002' then ifnull(T4."UsrNumber2",' ') else 'B' end	as "TipoCuentaAbono",
			ifnull(replace(case when T4."BankCode" = '002' 
			then T4."Account" else T4."U_EXM_INTERBANCARIA"end ,'-',''),'')				as "NroCtaAbono",
			'1'																			as "ModalidadDePago",
			ifnull(case T3."U_EXX_TIPODOCU" 
			when '4' then '3'
			when '7' then '4'
			else T3."U_EXX_TIPODOCU" END,'') 												as "TipoDocumentoProv",
			T3."LicTradNum"																as "NroDocProv",
			'   '																		as "CorrDocProv",
			T3."CardName"																as "NombreProveedor",
			ifnull(T1."U_EXP_NROSUNAT",'')												as "ReferenciaProveedor",
			ifnull(T1."U_EXP_NROSUNAT",'')												as "ReferenciaEmpresa",
			case when T1."U_EXP_MONEDA" = 'SOL' then '0001' else '1001' end				as "Moneda",
			TO_DECIMAL(T1."U_EXP_IMPORTE",14,2)											as "Importe",
			'S'																			as "Validar",
			''																			as "Filler"
		--from OVPM T0 
		from "@EXP_PMP1"		T1 
		inner join "@EXP_OPMP"	T2 on T1."DocEntry" = T2."DocEntry"
		inner join OCRD			T3 on T1."U_EXP_CARDCODE" = T3."CardCode"
		inner join OCRB			T4 on T4."CardCode" = T3."CardCode" and T1."U_EXP_MONEDA" = T4."UsrNumber1"
		where T2."DocEntry" = :NroPM 
		and T1."U_EXP_COD_SUCURSAL" = :NroSC
		and T1."U_EXP_CODCTABANCO" = :NroCT
		and coalesce(T1."U_EXP_SLC_PAGO",'') = 'Y'
		and T4."U_EXC_ACTIVO" = 'Y'
	)
	/*,
	CTE_BENEF as
	(
		select 
			'3'						as "TipoRegistro",
			case when T1."U_EXP_TIPODOC" = '18' then 'F' else 'D' end						as "TipoDocumento",
			(select TX0."FolioPref"||TX0."FolioNum" from OPCH TX0 where TX0."DocEntry" = T1."U_EXP_DOCENTRYDOC" and TX0."ObjType" = T1."U_EXP_TIPODOC")	as "NroDocAPagar",
			0.01/*T0."DocTotal"*//*			as "Importe"
		from 
		"@EXP_PMP1"					T1 
		inner join "@EXP_OPMP"		T2 on T1."DocEntry" = T2."DocEntry"
		--inner join VPM2				T3 on T0."DocEntry"	= T3."DocNum" 
		where T2."DocEntry" = :NroPM
	)*/
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
		lpad(to_bigint(right(trim("NroCtaCargo"),10))+(select sum(to_bigint(right(trim(TX0."NroCtaAbono")
		,case when "TipoCuentaAbono" = 'A' then 11 else 10 end))) from CTE_PROV TX0 where ifnull(TX0."NroCtaAbono",'') <>''),15,'0')||
		--rpad("Filler",100,' ') ||
		--lpad("NroPlanilla",6,'0') ||
		--lpad((select count('A') from CTE_PROV),6,' ') ||
		--lpad('0',6,' ') ||
		--lpad("Estado",80,' ')
		'Z' as "Data"
	from CTE_CAB
	union all 
	select 
		"TipoRegistro"			||
		"TipoCuentaAbono"		||
		rpad("NroCtaAbono",20,' ') ||
		"ModalidadDePago"		||
		"TipoDocumentoProv"		||
		rpad("NroDocProv",12,' ')			||
		"CorrDocProv"			||
		rpad(left("NombreProveedor",75),75,' ')		||
		rpad(left("ReferenciaProveedor",40),40,' ')	||
		rpad(left("ReferenciaEmpresa",20),20,' ')	||
		"Moneda"				||
		lpad(round("Importe",2),17,'0')				||
		"Validar"
		||'Z' as "Data"
	from CTE_PROV where ifnull("NroCtaAbono",'') <>'';
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