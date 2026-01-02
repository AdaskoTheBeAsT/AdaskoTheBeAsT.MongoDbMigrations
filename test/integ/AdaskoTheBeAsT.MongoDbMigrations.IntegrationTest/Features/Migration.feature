Feature: Database Migration
    As a developer
    I want to migrate my MongoDB database
    So that I can evolve the database schema safely

Background:
    Given a MongoDB database with clients collection
    And the collection has the following documents
        | name | age |
        | Alex | 17  |
        | Max  | 25  |

Scenario: Migrate database to specific version
    When I run migration to version "1.0.0"
    Then the migration should succeed
    And the database version should be "1.0.0"

Scenario: Migrate database to latest version
    When I run migration to latest version
    Then the migration should succeed
    And the database version should be "1.1.0"

Scenario: Rollback database migration
    Given the database is at version "1.1.0"
    When I rollback 1 migration step
    Then the migration should succeed
    And the database version should be "1.0.0"

Scenario: Dry run does not apply changes
    When I run dry run migration to version "1.0.0"
    Then the migration should succeed
    And the result should indicate dry run
    And the documents should still have "name" field
