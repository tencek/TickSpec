Feature: Web login testing

    Scenario: Successful login using <browser>
    Given I use browser <browser>
    When I try to login using username <username> and password <password>
    Then I am logged in

    Examples:
    | username      | password      |
    | test          | psw           |
    | test2         | psw2          |

    Scenario: Show page using <browser>
    Given I use browser <browser>
    When I go to the main page
    Then The main page is displayed

@Smoke
Shared Examples:
| browser       |
| Firefox       |

@Regression
Shared Examples:
| browser       |
| Firefox       |
| Explorer      |
| Chrome        |