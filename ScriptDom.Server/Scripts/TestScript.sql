insert into dbo.FinalTable (Field1, Field2, Field3)

select
    TA.Field1A
    ,TA.Field2A
    ,TA.Field3A
from dbo.TableA as TA

union all

select
    TB.Field1B
    ,TB.Field2B
    ,TB.Field3B
from dbo.TableB as TB