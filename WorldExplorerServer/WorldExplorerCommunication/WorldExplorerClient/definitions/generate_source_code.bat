dotnet tool install -g Confluent.Apache.Avro.AvroGen
avrogen -s delete_object.asvc ./../schemas/
avrogen -s new_object.asvc ./../schemas/
avrogen -s presence.asvc ./../schemas/
avrogen -s update_object.asvc ./../schemas/
avrogen -s view.asvc ./../schemas/ 
avrogen -s zoom.asvc ./../schemas/
avrogen -s table.asvc ./../schemas/
echo Finished
pause