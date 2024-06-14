CREATE PROCEDURE EXD_SP_PM_LISTAR_SERIES_X_SUCURSAL()
AS
BEGIN
	select 
		T0."BPLId",
		T0."BPLName",
		coalesce(T0.U_EXX_RETPRO,'N') as "RetPro",
		(select MAX(TX0."Series") from NNM1 TX0 where TX0."BPLId" = T0."BPLId" and coalesce(TX0.U_EXC_CR,'') != 'Y' and TX0."ObjectCode" = '46') as "CodSerPago",
		(select MAX(TX0."SeriesName") from NNM1 TX0 where TX0."BPLId" = T0."BPLId" and coalesce(TX0.U_EXC_CR,'') != 'Y' and TX0."ObjectCode" = '46')  as "NomSerPago",
		(select MAX(TX0."Series") from NNM1 TX0 where TX0."BPLId" = T0."BPLId" and coalesce(TX0.U_EXC_CR,'') = 'Y' and TX0."ObjectCode" = '46') as "CodSerReten",
		(select MAX(TX0."SeriesName") from NNM1 TX0 where TX0."BPLId" = T0."BPLId" and coalesce(TX0.U_EXC_CR,'') = 'Y' and TX0."ObjectCode" = '46') as "NomSerReten"
	from OBPL T0 order by 1;
END
