DROP FUNCTION if exists public.rsr_f_add_station(int8, text, text, text, float8, text, float8);

CREATE OR REPLACE FUNCTION public.rsr_f_add_station(p_codeid bigint, p_direction text, p_station_type text, p_title text, p_longitude double precision, p_transport_type text, p_latitude double precision)
 RETURNS bigint
 LANGUAGE plpgsql
AS $function$
declare
    tmp_station_id bigint;
begin
    select s.id
    into tmp_station_id
    from public.station s
    where s.codeid = p_codeid;
    if tmp_station_id is null then
        insert into public.station (direction, codeid, station_type, title, longitude, transport_type, latitude)
        SELECT p_direction, p_codeid, p_station_type, p_title, p_longitude, p_transport_type, p_latitude
        returning id into tmp_station_id;
    else

        UPDATE public.station
        SET direction=p_direction,
            station_type=p_station_type,
            title=p_title,
            longitude=p_longitude,
            transport_type=p_transport_type,
            latitude=p_latitude
        WHERE id = tmp_station_id;
    end if;
    return tmp_station_id;
end;
$function$
;

-- Permissions

ALTER FUNCTION public.rsr_f_add_station(int8, text, text, text, float8, text, float8) OWNER TO dbo;
GRANT ALL ON FUNCTION public.rsr_f_add_station(int8, text, text, text, float8, text, float8) TO dbo;