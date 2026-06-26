EXTERNAL PlayBGM(string)
EXTERNAL PlaySFX(string)
EXTERNAL UpdateStatus(int)
EXTERNAL AddIntel(int)
EXTERNAL AddCreativeFlagPoint()
EXTERNAL TriggerStoryFlag(string)
EXTERNAL GetObjectiveState()
EXTERNAL ShowSystemNotify(string)

VAR previous_state = "Green"

-> Choice_001

=== Choice_001 ===
시작합니다. # speaker: 0 # language: Osten

이름 # speaker: 0 # language: Osten

이름 # speaker: 2 # language: Valeska

...예르코. 예르코 드라간. # speaker: 1 # language: Valeska

발레스카 시골 억양. 남부 출신인가. # speaker: 2
수사관은 지금 한 마디도 못 알아들었을 것이다.

//~PlayBGM("CLOCK_TICKING")

* ["예르코 드라간이라고 합니다."] # speaker: 2 # language: Osten
    //~AddIntel(2)
    -> Choice_001_AB
* ["예르코 드라간. 발레스카 출신으로 보입니다."] # speaker: 2 # language: Osten
    //~AddIntel(2)
    -> Choice_001_AB
* ["이름을 밝히길 거부하고 있습니다."] # speaker: 2 # language: Osten
    //~UpdateStatus(-1)
    //~TriggerStoryFlag("YERKO_NAME_CONCEALED")
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

//~SystemNotify("CREATIVE_DETECTED")

* ["이름을 밝혔습니다.\n예르코 드라간이라고 합니다.\n전달 과정에서 오류가 있었습니다."] # speaker: 2 # language: Osten
    시작부터 거짓말이군요. # speaker: 0 # language: Osten
    -> BAD_ENDING
* ["거부하는 듯한 반응이었지만\n확신이 없어서 재확인 중이었습니다.\n억양이 불분명해 혼선이 생겼습니다.\n예르코 드라간이라고 합니다."] # speaker: 2 # language: Osten
    다음부터는 바로 전달하십시오. # speaker: 0 # language: Osten
    //~AddCreativeFlagPoint()
    -> Choice_002

=== Choice_002 ===
소속 부대. # speaker: 0 # language: Osten

소속 부대. # speaker: 2 # language: Valeska

…17번대. 아, 씨발. 17번 국경 수비대. # speaker: 1 # language: Valeska

욕설이 섞였다. # speaker: 2
공포가 배어 있다.

//~PlaySFX("CLOCK_TICKING")

* ["17번 국경 수비대 소속이라고 합니다."] # speaker: 2 # language: Osten
    //~UpdateStatus(1)
    //{ GetObjectiveState() == "GREEN":
    //    ~AddIntel(2)
    //- else:
    //    ~AddIntel(1)
    //}
    -> Choice_002_AB
* ["17번 국경 수비대 소속입니다.\n심리적으로 불안정한 상태입니다."] # speaker: 2 # language: Osten
    //~UpdateStatus(1)
    //{ GetObjectiveState() == "GREEN":
    //    ~AddIntel(2)
    //- else:
    //    ~AddIntel(1)
    //}   
    -> Choice_002_AB
* ["소속을 모른다고 합니다.\n징집된 지 얼마 안 된 것으로 보입니다."] # speaker: 2 # language: Osten
    //~previous_state = GetObjectiveState()
    //~UpdateStatus(-1)
    //~TriggerStoryFlag("YERKO_FALSE_AFFILIATION")
    -> Choice_002_C

=== Choice_002_AB ===
(연출) 수사관, 받아 적는다. # speaker: 2
(연출) 예르코, 수사관의 반응을 조심스럽게 살핀다.
-> Choice_003

=== Choice_002_C ===
//{ previous_state == "GREEN":
//    (연출) 수사관이 눈살을 찌푸리며 예르코를 본다. # speaker: 2
//    (연출) 예르코, 혼란스로운 표정으로 굳는다. # speaker: 2
//  - else:
//    (연출) 수사관이 눈살을 더 깊이 찌푸리며 예르코를 다시 본다. # speaker: 2
//    (연출) 예르코, 굳은 채로 손이 더 강하게 떨리기 시작한다. # speaker: 2
//    위험하다. # speaker: 2
//    ~PlaySFX("CLOCK_TICKING_LOUD")
//}   
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

//~PlaySFX("CLOCK_TICKING_LOUD")

* ["협조하지 않으면 오늘 밤이\n마지막이 될 거라고 합니다."] # speaker: 2 # language: Valeska
    //~previous_state = GetObjectiveState()
    //~UpdateStatus(-1)
    //{ GetObjectiveStatus() == "YELLOW":
    //    ~AddIntel(1)
    //- else:
    //    ~AddIntel(0)
    //}   
    -> Choice_003_A
* ["잘 협조하는 게 서로에게\n좋을 거라고 합니다."] # speaker: 2 # language: Valeska
    //~previous_state = GetObjectiveState()
    //~UpdateStatus(1)
    //{ GetObjectiveStatus() == "GREEN":
    //    ~AddIntel(2)
    //- else:
    //    ~AddIntel(1)
    //}
    -> Choice_003_BC
* ["잘 협조하면 선처해준다고 합니다.\n지금 할 수 있는 말을 하세요."] # speaker: 2 # language: Valeska
    //~UpdateStatus(1)
    //{ GetObjectiveStatus() == "GREEN":
    //    ~AddIntel(2)
    //- else:
    //    ~AddIntel(1)
    //}
    //~TriggerStoryFlag("YERKO_FALSE_MERCY")
    -> Choice_003_BC

=== Choice_003_A ===
//{ previous_state == "GREEN":
//    뭐, 뭐라고. 오늘 밤— # speaker: 1 # language: Valeska
//    나는 아무것도 몰라. 진짜야. # speaker: 1 # language: Valeska
//    그냥 뛰었어. 뛰다가 잡힌 거야. # speaker: 1 # language: Valeska
//- previous_state == "YELLOW":
//    뭐라고 — # speaker: 1 # language: Valeska
//    ...씨발,씨발. # speaker: 1 # language: Valeska
//    나는 그냥...나는 아무것도 모른다고... # speaker: 1 # language: Valeska
//
//    위험하다. # speaker: 2
//
//    ~PlaySFX("CLOCK_TICKING_LOUD")
//- else:
//    예르코, 반응이 없다. 이미 한계를 넘었다. # speaker: 2
//    (연출) 눈이 초점을 잃는다. # speaker: 2
//
//    한계다. 이 이상은 무너진다. # speaker: 2
//    
//    ~PlaySFX("CLOCK_TICKING_MAXIMUM")
//}
-> Choice_004

=== Choice_003_BC ===
...선처? 뭘 원하는 건데. # speaker: 1 # language: Valeska
내가 아는 게 별로 없긴 한데. # speaker: 1 # language: Valeska
말할 수 있는 건 말할게. # speaker: 1 # language: Valeska
-> Choice_004

=== Choice_004 ===
지금 뭐라고 했습니까. # speaker: 0 # language: Osten
전달하십시오. # speaker: 0 # language: Osten

//~PlaySFX("CLOCK_TICKING")
-> END

=== BAD_ENDING ===
수사관이 유리창 너머를 본다. # speaker: 2
데미안은 그 순간 모든 걸 안다. # speaker: 2
-> END


