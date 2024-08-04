DROP FUNCTION if exists public.rsr_f_add_region(int8, text);

CREATE OR REPLACE FUNCTION public.rsr_f_add_region(p_code_id bigint, p_title text)
    RETURNS bigint
    LANGUAGE plpgsql
AS
$function$
declare
    tmp_region_id bigint;
begin

    select r.id
    into tmp_region_id
    from public.region r
    where r.codeid = p_code_id;
    if tmp_region_id is null
    then
        insert into public.region (title, codeid)
        values (p_title, p_code_id)
        returning region.id into tmp_region_id;
    else
        update public.region r
        set title = p_title
        where r.id = tmp_region_id;
    end if;
    return tmp_region_id;
end;
$function$
;

-- Permissions

ALTER FUNCTION public.rsr_f_add_region(int8, text) OWNER TO dbo;
GRANT ALL ON FUNCTION public.rsr_f_add_region(int8, text) TO dbo;