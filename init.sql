CREATE TABLE Job (
  ID UUID PRIMARY KEY,
  Value INT DEFAULT 0,
  CurrentWorker VARCHAR
);


CREATE INDEX idx_CurrentWorker ON Job(CurrentWorker);