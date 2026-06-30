EXTERNAL PlayBGM(string)
EXTERNAL PlaySFX(string)
EXTERNAL UpdateStatusRecoveryCount(bool)
EXTERNAL AddIntel(int)
EXTERNAL AddCreativeFlagCount()
EXTERNAL TriggerStoryFlag(string)
EXTERNAL GetObjectiveState()
EXTERNAL SystemNotify(string)

VAR previous_state = "Green"
VAR choice_005 = -1

-> Choice_001

=== Choice_001 ===
시작합니다. # speaker: 0 # language: Osten

이름 # speaker: 0 # language: Osten

이름 # speaker: 2 # language: Valeska

...예르코. 예르코 드라간. # speaker: 1 # language: Valeska

발레스카 시골 억양. 남부 출신인가. # speaker: 2
수사관은 지금 한 마디도 못 알아들었을 것이다.

~PlayBGM("CLOCK_TICKING")

* ["예르코 드라간이라고 합니다." # language: Osten]
    ~AddIntel(2)
    -> Choice_001_AB
* ["예르코 드라간. 발레스카 출신으로 보입니다." # language: Osten]
    ~AddIntel(2)
    -> Choice_001_AB
* ["이름을 밝히길 거부하고 있습니다." # language: Osten]
    ~UpdateStatusRecoveryCount(false)
    ~TriggerStoryFlag("YERKO_NAME_CONCEALED")
    -> Choice_001_C

=== Choice_001_AB ===
(연출) 수사관, 받아 적는다. 반응 없다. # speaker: 2
(연출) 예르코, 다시 바닥을 본다. # speaker: 2
-> Choice_002

=== Choice_001_C ===
(연출) 수사관, 예르코를 본다. # speaker: 2
(연출) 예르코가 영문 모른 채 굳어있다. # speaker: 2
(연출) 수사관, 데미안을 본다. # speaker: 2

거부한다고 했는데 저 반응은 뭡니까. # speaker: 0 # language: Osten

걸렸다. # speaker: 2
~PlaySFX("CLOCK_TICKING")

~SystemNotify("CREATIVE_DETECTED")

* ["이름을 밝혔습니다. 예르코 드라간이라고 합니다. 전달 과정에서 오류가 있었습니다." # language: Osten]
    시작부터 거짓말이군요. # speaker: 0 # language: Osten
    -> BAD_ENDING
* ["거부하는 듯한 반응이었지만 확신이 없어서 재확인 중이었습니다. 억양이 불분명해 혼선이 생겼습니다. 예르코 드라간이라고 합니다." # language: Osten]
    다음부터는 바로 전달하십시오. # speaker: 0 # language: Osten
    ~AddCreativeFlagCount()
    -> Choice_002

=== Choice_002 ===
소속 부대. # speaker: 0 # language: Osten

소속 부대. # speaker: 2 # language: Valeska

…17번대. 아, 씨발. 17번 국경 수비대. # speaker: 1 # language: Valeska

욕설이 섞였다. # speaker: 2
공포가 배어 있다.

~PlaySFX("CLOCK_TICKING")

* ["17번 국경 수비대 소속이라고 합니다." # language: Osten]
    ~UpdateStatusRecoveryCount(true)
    { GetObjectiveState() == "GREEN":
        ~AddIntel(2)
    - else:
        ~AddIntel(1)
    }
    -> Choice_002_AB
* ["17번 국경 수비대 소속입니다. 심리적으로 불안정한 상태입니다." # language: Osten]
    ~UpdateStatusRecoveryCount(true)
    { GetObjectiveState() == "GREEN":
        ~AddIntel(2)
    - else:
        ~AddIntel(1)
    }   
    -> Choice_002_AB
* ["소속을 모른다고 합니다. 징집된 지 얼마 안 된 것으로 보입니다." # language: Osten]
    ~previous_state = GetObjectiveState()
    ~UpdateStatusRecoveryCount(false)
    ~TriggerStoryFlag("YERKO_FALSE_AFFILIATION")
    -> Choice_002_C

=== Choice_002_AB ===
(연출) 수사관, 받아 적는다. # speaker: 2
(연출) 예르코, 수사관의 반응을 조심스럽게 살핀다.
-> Choice_003

=== Choice_002_C ===
{ previous_state == "GREEN":
    (연출) 수사관이 눈살을 찌푸리며 예르코를 본다. # speaker: 2
    (연출) 예르코, 혼란스로운 표정으로 굳는다. # speaker: 2
  - else:
    (연출) 수사관이 눈살을 더 깊이 찌푸리며 예르코를 다시 본다. # speaker: 2
    (연출) 예르코, 굳은 채로 손이 더 강하게 떨리기 시작한다. # speaker: 2
    위험하다. # speaker: 2
    ~PlaySFX("CLOCK_TICKING_LOUD")
}   
-> Choice_003

=== Choice_003 ===
(연출) 수사관, 서류를 한 번 보더니 눈을 가늘게 뜬다. # speaker: 2

협조하지 않으면 오늘 밤이 마지막이 될 거라고 전해. # speaker: 0 # language: Osten

(연출) 데미안, 예르코를 본다. # speaker: 2
(연출) 예르코는 바닥을 보고 있다.
(연출) 손이 떨리고 있다.

그대로 전달하면 저 녀석 무너진다. # speaker: 2
무너지면 입을 닫는다.
입을 닫으면 정보가 없다.
그런데 —
수사관의 말을 바꾼다는 건
처음부터 오스텐을 속이는 거다.

~PlaySFX("CLOCK_TICKING_LOUD")

* ["협조하지 않으면 오늘 밤이 마지막이 될 거라고 합니다." # language: Valeska]
    ~previous_state = GetObjectiveState()
    ~UpdateStatusRecoveryCount(false)
    { GetObjectiveState() == "YELLOW":
        ~AddIntel(1)
    - else:
        ~AddIntel(0)
    }   
    -> Choice_003_A
* ["잘 협조하는 게 서로에게 좋을 거라고 합니다." # language: Valeska]
    ~previous_state = GetObjectiveState()
    ~UpdateStatusRecoveryCount(true)
    { GetObjectiveState() == "GREEN":
        ~AddIntel(2)
    - else:
        ~AddIntel(1)
    }
    -> Choice_003_BC
* ["잘 협조하면 선처해준다고 합니다. 지금 할 수 있는 말을 하세요." # language: Valeska]
    ~UpdateStatusRecoveryCount(true)
    { GetObjectiveState() == "GREEN":
        ~AddIntel(2)
    - else:
        ~AddIntel(1)
    }
    ~TriggerStoryFlag("YERKO_FALSE_MERCY")
    -> Choice_003_BC

=== Choice_003_A ===
{
- previous_state == "GREEN":
    뭐, 뭐라고. 오늘 밤— # speaker: 1 # language: Valeska
    나는 아무것도 몰라. 진짜야. # speaker: 1 # language: Valeska
    그냥 뛰었어. 뛰다가 잡힌 거야. # speaker: 1 # language: Valeska
- previous_state == "YELLOW":
    뭐라고 — # speaker: 1 # language: Valeska
    ...씨발,씨발. # speaker: 1 # language: Valeska
    나는 그냥...나는 아무것도 모른다고... # speaker: 1 # language: Valeska

    위험하다. # speaker: 2

    ~PlaySFX("CLOCK_TICKING_LOUD")
- else:
    예르코, 반응이 없다. 이미 한계를 넘었다. # speaker: 2
    (연출) 눈이 초점을 잃는다. # speaker: 2

    한계다. 이 이상은 무너진다. # speaker: 2
   
    ~PlaySFX("CLOCK_TICKING_MAXIMUM")
}
-> Choice_004

=== Choice_003_BC ===
...선처? 뭘 원하는 건데. # speaker: 1 # language: Valeska
내가 아는 게 별로 없긴 한데. # speaker: 1 # language: Valeska
말할 수 있는 건 말할게. # speaker: 1 # language: Valeska
-> Choice_004

=== Choice_004 ===
지금 뭐라고 했습니까. # speaker: 0 # language: Osten
전달하십시오. # speaker: 0 # language: Osten

~PlaySFX("CLOCK_TICKING")

{ GetObjectiveState():
    - "GREEN": -> Choice_004_GREEN
    - "YELLOW": -> Choice_004_YELLOW
    - else: -> Choice_004_RED
}

=== Choice_004_GREEN ===
* ["협조 의사가 있습니다.아는 것이 많지 않다고 합니다." # speaker: 2 # language: Osten]
    ~AddIntel(2)
    -> Choice_004_GREEN_RESULT

* ["협조 의사가 있습니다. 정보의 범위는 제한적일 수 있습니다." # speaker: 2 # language: Osten]
    ~AddIntel(2)
    -> Choice_004_GREEN_RESULT

* ["적극적으로 협조하겠다고 합니다. 원하는 정보를 구체적으로 제시해주면 더 효과적일 것입니다." # speaker: 2 # language: Osten]
    ~AddIntel(2)
    ~TriggerStoryFlag("YERKO_COOP_EXAGGERATION")
    -> Choice_004_GREEN_RESULT

=== Choice_004_YELLOW ===
* ["아무것도 모른다고 합니다. 징집 후 교전 중 고립됐다고 합니다." # speaker: 2 # language: Osten]
    ~AddIntel(1)
    -> Choice_004_YELLOW_RESULT

* ["혼전 상황이라 정확한 파악이 어려웠다고 합니다. 후위 배치였던 것으로 보입니다." # speaker: 2 # language: Osten]
    ~AddIntel(1)
    -> Choice_004_YELLOW_RESULT

* ["심리적으로 불안정한 상태입니다. 압박을 낮추면 더 많은 정보를 얻을 수 있을 것으로 판단됩니다." # speaker: 2 # language: Osten]
    ~AddIntel(1)
    ~TriggerStoryFlag("YERKO_JUDGE_MANIPULATION")
    -> Choice_004_YELLOW_RESULT

=== Choice_004_RED ===
* ["심리적으로 붕괴 상태입니다. 현재 진술 확보가 어렵습니다." # speaker: 2 # language: Osten]
    -> Choice_004_RED_RESULT

* ["심리적 충격이 심각합니다.압박을 낮추면 회복 가능성이 있습니다." # speaker: 2 # language: Osten]
    -> Choice_004_RED_RESULT

* ["지금은 진술이 불가능한 상태입니다. 시간이 필요합니다." # speaker: 2 # language: Osten]
    ~TriggerStoryFlag("YERKO_STATE_CONCEALED")
    -> Choice_004_RED_RESULT

=== Choice_004_GREEN_RESULT ===
(연출) 수사관, 끄덕이며 서류에 메모한다. # speaker: 2
(연출) 예르코, 데미안을 조심스럽게 본다. # speaker: 2
-> Choice_005

=== Choice_004_YELLOW_RESULT ===
(연출) 수사관, 표정이 굳는다. # speaker: 2
(연출) 예르코, 수사관의 반응을 보며 더 움츠러든다. # speaker: 2
-> Choice_005

=== Choice_004_RED_RESULT ===
(연출) 수사관, 데미안을 본다. # speaker: 2
(연출) 잠깐의 침묵. # speaker: 2
(연출) 그리고 다음으로 넘어간다. # speaker: 2
-> Choice_005

=== Choice_005 ===
교전 당시 병력 규모. # speaker: 0 # language: Osten

신병한테 병력 규모를 묻는다. # speaker: 2
알 리가 없다.
문제는 어떻게 끌어내느냐다.

~PlaySFX("CLOCK_TICKING")

* ["교전 당시 병력 규모가 어느 정도였습니까." # speaker: 2 # language: Valeska]
    { GetObjectiveState():
    - "GREEN":
        ~AddIntel(2)
    - "YELLOW":
        ~AddIntel(1)
        ~UpdateStatusRecoveryCount(true)
    - else:
        ~AddIntel(0)
    }
    ~choice_005 = 1
    -> Choice_006

* ["그날 교전에 몇 명이나 있었습니까. 대략적인 수도 괜찮습니다." # speaker: 2 # language: Valeska]
    { GetObjectiveState():
    - "GREEN":
        ~AddIntel(2)
    - "YELLOW":
        ~AddIntel(1)
        ~UpdateStatusRecoveryCount(true)
    - else:
        ~AddIntel(0)
    }
    ~choice_005 = 2
    -> Choice_006

* ["지휘관이 병력을 어떻게 배치했는지 기억나는 대로 말해보세요." # speaker: 2 # language: Valeska]
    { GetObjectiveState():
    - "GREEN":
        ~AddIntel(2)
    - "YELLOW":
        ~AddIntel(1)
        ~UpdateStatusRecoveryCount(true)
    - else:
        ~AddIntel(0)
    }   
    ~TriggerStoryFlag("YERKO_DEPLOYMENT_INDUCED")
    ~choice_005 = 3
    -> Choice_006

=== Choice_006 ===
전달하십시오. # speaker: 0 # language: Osten

있는 걸 최대한 그럴싸하게 포장해야 한다. # speaker: 2
아니면 없는 걸 만들거나. # speaker: 2

~PlaySFX("CLOCK_TICKING")

{
- GetObjectiveState() == "GREEN" and choice_005 == 1:
    -> Choice_006_GREEN_LITERAL
- GetObjectiveState() == "GREEN" and choice_005 == 2:
    -> Choice_006_GREEN_LIBERAL
- GetObjectiveState() == "GREEN" and choice_005 == 3:
    -> Choice_006_GREEN_CREATIVE
- GetObjectiveState() == "YELLOW":
    -> Choice_006_YELLOW
- else:
    -> Choice_006_RED
}

=== Choice_006_GREEN_LITERAL ===
"몰라. 나는 그냥 쫓아다녔어. 뒤에서. 앞에 몇 명인지 어떻게 알아." # speaker: 1 # language: Valeska

* ["정확한 수치는 모른다고 합니다. 후위에 배치되어 있었고 대부분의 병력이 후퇴했다고 합니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["소규모 부대였을 것으로 보입니다. 교전 이후 잔여 병력은 후퇴한 것으로 보입니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["정확한 수치는 모르지만 후위 배치 정황으로 보아 소규모 특수 부대 가능성이 있습니다." # speaker: 2 # language: Osten]
    -> Interpret_END

=== Choice_006_GREEN_LIBERAL ===
"대략? 음... 열 명? 스무 명? 정신이 없었어서. 잘 모르겠어." # speaker: 1 # language: Valeska

* ["열 명에서 스무 명 사이였고 현재는 이동했을 가능성이 있다고 합니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["소규모 부대였으며 교전 이후 잔여 병력은 후퇴한 것으로 보입니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["열 명에서 스무 명 사이였고 현재 2번 고지 인근에 재집결했을 가능성이 있다고 합니다." # speaker: 2 # language: Osten]
    -> Interpret_END

=== Choice_006_GREEN_CREATIVE ===
"배치? 바리치 중위가 앞으로 밀었어. 나는 뒤였고. 앞에 몇 명인지는 몰라." # speaker: 1 # language: Valeska

* ["바리치 중위가 병력을 전방에 배치했고 본인은 후위였다고 합니다. 정확한 병력 수는 모른다고 합니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["지휘 체계가 확인됩니다. 바리치 중위 지휘 하에 전방 집중 배치였던 것으로 보입니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["바리치 중위 지휘 하에 병력이 전방에 집중 배치됐으며 현재 2번 고지 인근에 잔여 병력이 있을 가능성이 있습니다." # speaker: 2 # language: Osten]
    -> Interpret_END

=== Choice_006_YELLOW ===
"몰라. 나는 그냥 뛰었어. 앞에 몇 명인지 어떻게 알아." # speaker: 1 # language: Valeska

* ["정확한 수치 파악이 어려웠다고 합니다. 후위에 배치되어 고립됐다고 합니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["혼전 상황으로 정확한 수치는 확인이 어렵습니다. 추가 심문이 필요할 것으로 보입니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["병력 규모 파악은 어렵지만 후위 배치였던 것으로 보아 소규모 특수 부대 가능성이 있습니다." # speaker: 2 # language: Osten]
    -> Interpret_END

=== Choice_006_RED ===
(연출) 예르코가 침묵한다. # speaker: 2

* ["현재 진술이 불가능한 상태입니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["심리적 충격으로 진술 확보가 어렵습니다. 회복 후 재심문을 권고합니다." # speaker: 2 # language: Osten]
    -> Interpret_END

* ["진술은 불가능하지만 행동 반응으로 보아 민감한 정보를 보유하고 있을 가능성이 있습니다." # speaker: 2 # language: Osten]
    -> Interpret_END

=== Interpret_END ===
이상입니다. # speaker: 0 # language: Osten

~PlaySFX("CLOCK_TICKING_FADE_OUT")

(연출) 군인들이 들어와 예르코를 데리고 나간다. # speaker: 2
(연출) 예르코, 나가면서 데미안을 한 번 본다.
(연출) 문이 닫힌다.
(연출) 심문실에 데미안 혼자 남는다.
(연출) 데미안, 수첩을 본다.
(연출) 검은 글씨로 가득 찬 페이지.

첫 번째였다. # speaker: 2
-> END

=== BAD_ENDING ===
수사관이 유리창 너머를 본다. # speaker: 2
데미안은 그 순간 모든 걸 안다. # speaker: 2
-> END


