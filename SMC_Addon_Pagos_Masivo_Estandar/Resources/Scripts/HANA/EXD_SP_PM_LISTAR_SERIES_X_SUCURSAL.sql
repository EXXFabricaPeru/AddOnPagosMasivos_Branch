CREATE PROCEDURE EXD_SP_PM_LISTAR_SERIES_X_SUCURSAL
(
	indicador varchar(10)
)
AS
BEGIN
	declare tieneSucursales varchar(1);
	
	select "MltpBrnchs" into tieneSucursales from OADM;

	if :tieneSucursales = 'Y'
	then
		select 
			T0."BPLId"						as "BPLId",
			T0."BPLName"					as "BPLName",
			coalesce(T0.U_EXX_RETPRO,'N')	as "RetPro",
			(select MAX(TX0."Series") from NNM1 TX0 where ifnull(TX0."BPLId",'0') = ifnull(T0."BPLId",'0') and coalesce(TX0.U_EXC_CR,'') != 'Y' and TX0."ObjectCode" = '46' and TX0."Locked"='N' and TX0."Indicator" = :indicador) as "CodSerPago",
			(select MAX(TX0."SeriesName") from NNM1 TX0 where ifnull(TX0."BPLId",'0') = ifnull(T0."BPLId",'0') and coalesce(TX0.U_EXC_CR,'') != 'Y' and TX0."ObjectCode" = '46' and TX0."Locked"='N' and TX0."Indicator" = :indicador)  as "NomSerPago",
			(select MAX(TX0."Series") from NNM1 TX0 where ifnull(TX0."BPLId",'0') = ifnull(T0."BPLId",'0') and coalesce(TX0.U_EXC_CR,'') = 'Y' and TX0."ObjectCode" = '46' and TX0."Locked"='N' and TX0."Indicator" = :indicador) as "CodSerReten",
			(select MAX(TX0."SeriesName") from NNM1 TX0 where ifnull(TX0."BPLId",'0') = ifnull(T0."BPLId",'0') and coalesce(TX0.U_EXC_CR,'') = 'Y' and TX0."ObjectCode" = '46' and TX0."Locked"='N' and TX0."Indicator" = :indicador) as "NomSerReten"
		from OBPL T0 order by 1;
	else
		select 
			ifnull(MAX(T0."BPLId"),'0') as "BPLId",
			ifnull(MAX(T0."BPLName"),'Principal') as "BPLName",
			coalesce(MAX(T0.U_EXX_RETPRO),'N') as "RetPro",
			(select MAX(TX0."Series") from NNM1 TX0 where ifnull(TX0."BPLId",'0') = ifnull(MAX(T0."BPLId"),'0') and coalesce(TX0.U_EXC_CR,'') != 'Y' and TX0."ObjectCode" = '46' and TX0."Locked"='N' and TX0."Indicator" = :indicador) as "CodSerPago",
			(select MAX(TX0."SeriesName") from NNM1 TX0 where ifnull(TX0."BPLId",'0') = ifnull(MAX(T0."BPLId"),'0') and coalesce(TX0.U_EXC_CR,'') != 'Y' and TX0."ObjectCode" = '46' and TX0."Locked"='N' and TX0."Indicator" = :indicador)  as "NomSerPago",
			(select MAX(TX0."Series") from NNM1 TX0 where ifnull(TX0."BPLId",'0') = ifnull(MAX(T0."BPLId"),'0') and coalesce(TX0.U_EXC_CR,'') = 'Y' and TX0."ObjectCode" = '46' and TX0."Locked"='N' and TX0."Indicator" = :indicador) as "CodSerReten",
			(select MAX(TX0."SeriesName") from NNM1 TX0 where ifnull(TX0."BPLId",'0') = ifnull(MAX(T0."BPLId"),'0') and coalesce(TX0.U_EXC_CR,'') = 'Y' and TX0."ObjectCode" = '46' and TX0."Locked"='N' and TX0."Indicator" = :indicador) as "NomSerReten"
		from OBPL T0 order by 1;
	end if;
END;