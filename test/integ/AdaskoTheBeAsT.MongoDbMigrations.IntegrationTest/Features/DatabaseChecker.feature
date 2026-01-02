Feature: Database State Checker
    As a developer
    I want to check if my database is outdated
    So that I know when migrations need to run

Background:
    Given a MongoDB database with clients collection
    And the collection has the following documents
        | name | age |
        | Alex | 17  |
        | Max  | 25  |

Scenario: Database without migrations is outdated
    When I check if the database is outdated
    Then the result should indicate database is outdated

Scenario: Database after migrations is not outdated
    Given the database is at version "1.1.0"
    When I check if the database is outdated
    Then the result should indicate database is not outdated

Scenario: Throw exception when database is outdated
    When I call ThrowIfDatabaseOutdated
    Then a DatabaseOutdatedException should be thrown
