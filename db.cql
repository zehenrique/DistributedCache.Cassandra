CREATE KEYSPACE cassandracache WITH REPLICATION = { 'class' : 'SimpleStrategy','replication_factor' : 3 };

USE cassandracache;

CREATE TABLE IF NOT EXISTS cassandra_cache (
	id text,
	value blob,
	expiration_time timestamp,
    PRIMARY KEY (id)
) WITH gc_grace_seconds = 86400;