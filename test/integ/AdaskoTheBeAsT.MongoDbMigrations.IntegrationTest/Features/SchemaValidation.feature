Feature: Schema Validation
    As a developer
    I want to validate database schema during migration
    So that I can catch inconsistencies early

Background:
    Given a MongoDB database with clients collection

Scenario: Schema validation fails for inconsistent documents
    Given the collection has documents with inconsistent schema
        | name | isActive |
        | Alex | true     |
        | Max  |          |
    When I run migration to version "1.0.0" with schema validation expecting failure
    Then the migration should throw an exception

Scenario: Schema validation passes for consistent documents
    Given the collection has the following documents
        | name | age |
        | Alex | 17  |
        | Max  | 25  |
    When I run migration to version "1.0.0" with schema validation
    Then the migration should succeed
    And the database version should be "1.0.0"
