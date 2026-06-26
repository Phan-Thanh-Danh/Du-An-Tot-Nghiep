import { QuestionBankItem } from '@/types/content-council/questionBank'

const subjects = [
  { id: 1, code: 'WEB201', name: 'Lập trình Web cơ bản' },
  { id: 2, code: 'COM101', name: 'Nhập môn CNTT' },
  { id: 3, code: 'PRO192', name: 'Lập trình Java' },
  { id: 4, code: 'DBI202', name: 'Hệ quản trị CSDL' }
]

const generateId = () => Math.random().toString(36).substr(2, 9)

const mockQuestions: QuestionBankItem[] = []
let idCounter = 1

const getRandomElement = (arr: any[]) => arr[Math.floor(Math.random() * arr.length)]

// 1. Sinh 10 câu trắc nghiệm đơn cho WEB201
for (let i = 1; i <= 10; i++) {
  const choices = [
    { id: generateId(), content: `Lựa chọn A của câu hỏi ${i}` },
    { id: generateId(), content: `Lựa chọn B của câu hỏi ${i}` },
    { id: generateId(), content: `Lựa chọn C của câu hỏi ${i}` },
    { id: generateId(), content: `Lựa chọn D của câu hỏi ${i}` }
  ]
  mockQuestions.push({
    id: idCounter++,
    code: `Q-WEB201-${i.toString().padStart(3, '0')}`,
    subjectId: 1,
    subjectCode: 'WEB201',
    subjectName: 'Lập trình Web cơ bản',
    type: 'multiple_choice',
    selectionType: 'single',
    content: `<p>Nội dung câu hỏi HTML cho WEB201 số ${i}. Chọn đáp án đúng nhất.</p>`,
    choices,
    correctAnswerIds: [choices[1].id],
    answerExplanation: 'Vì đáp án B mô tả đúng khái niệm.',
    difficulty: getRandomElement(['easy', 'medium', 'hard']),
    status: i % 5 === 0 ? 'inactive' : 'active',
    usageCount: i % 3 === 0 ? 0 : Math.floor(Math.random() * 5) + 1,
    createdAt: new Date(Date.now() - Math.random() * 10000000000).toISOString(),
    updatedAt: new Date().toISOString()
  })
}

// 2. Sinh 8 câu trắc nghiệm nhiều lựa chọn cho COM101
for (let i = 1; i <= 8; i++) {
  const choices = [
    { id: generateId(), content: `Đặc điểm X của ${i}` },
    { id: generateId(), content: `Đặc điểm Y của ${i}` },
    { id: generateId(), content: `Đặc điểm Z của ${i}` },
    { id: generateId(), content: `Đặc điểm W của ${i}` }
  ]
  mockQuestions.push({
    id: idCounter++,
    code: `Q-COM101-${i.toString().padStart(3, '0')}`,
    subjectId: 2,
    subjectCode: 'COM101',
    subjectName: 'Nhập môn CNTT',
    type: 'multiple_choice',
    selectionType: 'multiple',
    content: `<p>Hãy chọn TẤT CẢ các đáp án đúng về phần mềm máy tính (Câu ${i})</p>`,
    choices,
    correctAnswerIds: [choices[0].id, choices[2].id],
    answerExplanation: 'Đáp án A và C bao gồm các thành phần quan trọng.',
    difficulty: getRandomElement(['medium', 'hard']),
    status: 'active',
    usageCount: i % 2 === 0 ? 0 : 2,
    createdAt: new Date(Date.now() - Math.random() * 10000000000).toISOString(),
    updatedAt: new Date().toISOString()
  })
}

// 3. Sinh 6 câu tự luận cho PRO192
for (let i = 1; i <= 6; i++) {
  mockQuestions.push({
    id: idCounter++,
    code: `Q-PRO192-${i.toString().padStart(3, '0')}`,
    subjectId: 3,
    subjectCode: 'PRO192',
    subjectName: 'Lập trình Java',
    type: 'essay',
    content: `<p>Giải thích mô hình OOP và viết một đoạn mã minh họa kế thừa bằng Java. (Câu ${i})</p>`,
    sampleAnswer: 'Mô hình OOP bao gồm Đóng gói, Kế thừa, Đa hình, Trừu tượng. Mã minh hoạ: class A extends B {}',
    answerExplanation: 'Sinh viên cần nêu đủ 4 tính chất và code đúng cú pháp',
    difficulty: 'hard',
    status: 'active',
    usageCount: 1,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  })
}

// 4. Sinh một số câu trộn lẫn cho DBI202
for (let i = 1; i <= 6; i++) {
  const isMultiple = i % 2 === 0
  const choices = isMultiple ? [
    { id: generateId(), content: 'SELECT' },
    { id: generateId(), content: 'UPDATE' },
    { id: generateId(), content: 'DROP' }
  ] : undefined

  mockQuestions.push({
    id: idCounter++,
    code: `Q-DBI202-${i.toString().padStart(3, '0')}`,
    subjectId: 4,
    subjectCode: 'DBI202',
    subjectName: 'Hệ quản trị CSDL',
    type: isMultiple ? 'multiple_choice' : 'essay',
    selectionType: isMultiple ? 'single' : undefined,
    content: `<p>${isMultiple ? 'Câu lệnh SQL nào dùng để truy vấn dữ liệu?' : 'Nêu định nghĩa và phân loại Normalization.'} (Câu ${i})</p>`,
    choices,
    correctAnswerIds: isMultiple ? [choices![0].id] : undefined,
    sampleAnswer: isMultiple ? undefined : 'Normalization giúp giảm thiểu dư thừa...',
    difficulty: getRandomElement(['easy', 'medium']),
    status: 'active',
    usageCount: 0,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  })
}

// Thêm 1 câu cụ thể dễ search như yêu cầu
mockQuestions.push({
  id: idCounter++,
  code: 'Q-WEB-001',
  subjectId: 1,
  subjectCode: 'WEB201',
  subjectName: 'Lập trình Web cơ bản',
  type: 'multiple_choice',
  selectionType: 'single',
  content: '<p>HTTP là gì?</p>',
  choices: [
    { id: generateId(), content: 'HyperText Transfer Protocol' },
    { id: generateId(), content: 'Hyper Text Transmission Process' }
  ],
  correctAnswerIds: [], // Sẽ gán sau để đảm bảo lấy đúng ID
  difficulty: 'easy',
  status: 'active',
  usageCount: 0,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString()
})
mockQuestions[mockQuestions.length - 1].correctAnswerIds = [mockQuestions[mockQuestions.length - 1].choices![0].id]

export const initialMockQuestions = mockQuestions
