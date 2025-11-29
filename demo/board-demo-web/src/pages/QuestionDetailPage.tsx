import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { questionsApi, answersApi } from '../api';
import { Question, Answer, QuestionStatus } from '../types';
import { useAuth } from '../contexts/AuthContext';

export function QuestionDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  
  const [question, setQuestion] = useState<Question | null>(null);
  const [answers, setAnswers] = useState<Answer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [newAnswer, setNewAnswer] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (id) {
      loadQuestion();
      loadAnswers();
    }
  }, [id]);

  const loadQuestion = async () => {
    setIsLoading(true);
    try {
      const data = await questionsApi.getById(Number(id));
      setQuestion(data);
    } catch (err) {
      setError('질문을 불러오는데 실패했습니다.');
    } finally {
      setIsLoading(false);
    }
  };

  const loadAnswers = async () => {
    try {
      const data = await answersApi.getByQuestionId(Number(id));
      setAnswers(data);
    } catch (err) {
      console.error('Failed to load answers:', err);
    }
  };

  const handleVoteQuestion = async () => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    try {
      const result = await questionsApi.vote(Number(id));
      setQuestion(prev => prev ? { ...prev, voteCount: result.voteCount } : null);
    } catch (err) {
      console.error('Failed to vote:', err);
    }
  };

  const handleAnswerSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newAnswer.trim() || !isAuthenticated) return;

    setIsSubmitting(true);
    try {
      await answersApi.create(Number(id), { content: newAnswer });
      setNewAnswer('');
      loadAnswers();
      setQuestion(prev => prev ? { ...prev, answerCount: prev.answerCount + 1 } : null);
    } catch (err) {
      console.error('Failed to create answer:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAcceptAnswer = async (answerId: number) => {
    try {
      await answersApi.accept(answerId);
      loadQuestion();
      loadAnswers();
    } catch (err) {
      console.error('Failed to accept answer:', err);
    }
  };

  const handleVoteAnswer = async (answerId: number) => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    try {
      const result = await answersApi.vote(answerId);
      setAnswers(prev => 
        prev.map(a => a.id === answerId ? { ...a, voteCount: result.voteCount } : a)
      );
    } catch (err) {
      console.error('Failed to vote answer:', err);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('ko-KR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusBadge = (status: QuestionStatus) => {
    switch (status) {
      case QuestionStatus.Open:
        return <span className="px-2 py-1 bg-green-100 text-green-700 text-sm font-medium rounded">미해결</span>;
      case QuestionStatus.Answered:
        return <span className="px-2 py-1 bg-blue-100 text-blue-700 text-sm font-medium rounded">해결됨</span>;
      case QuestionStatus.Closed:
        return <span className="px-2 py-1 bg-gray-100 text-gray-700 text-sm font-medium rounded">종료</span>;
      default:
        return null;
    }
  };

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-green-600 mx-auto"></div>
      </div>
    );
  }

  if (error || !question) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600 mb-4">{error || '질문을 찾을 수 없습니다.'}</p>
        <Link to="/questions" className="text-green-600 hover:underline">
          목록으로 돌아가기
        </Link>
      </div>
    );
  }

  const canAccept = user?.id === question.authorId;

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* 질문 */}
      <article className="bg-white rounded-lg shadow-sm p-6">
        <div className="flex gap-4">
          {/* 투표 */}
          <div className="flex-shrink-0 text-center">
            <button
              onClick={handleVoteQuestion}
              className="p-2 hover:bg-gray-100 rounded transition"
            >
              ▲
            </button>
            <div className="text-xl font-bold text-gray-800">{question.voteCount}</div>
            <div className="text-xs text-gray-500">추천</div>
          </div>

          {/* 질문 내용 */}
          <div className="flex-1">
            <div className="flex items-center gap-2 mb-2">
              {getStatusBadge(question.status)}
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-4">{question.title}</h1>
            
            <div className="prose max-w-none mb-4">
              <div className="whitespace-pre-wrap">{question.content}</div>
            </div>

            {question.tags && question.tags.length > 0 && (
              <div className="flex gap-2 flex-wrap mb-4">
                {question.tags.map((tag, i) => (
                  <span
                    key={i}
                    className="px-2 py-1 bg-blue-50 text-blue-600 text-sm rounded"
                  >
                    {tag}
                  </span>
                ))}
              </div>
            )}

            <div className="flex items-center gap-4 text-sm text-gray-500 pt-4 border-t">
              <span className="font-medium text-gray-700">{question.authorName}</span>
              <span>{formatDate(question.createdAt)}</span>
              <span>조회 {question.viewCount}</span>
            </div>
          </div>
        </div>
      </article>

      {/* 답변 섹션 */}
      <section>
        <h2 className="text-lg font-semibold mb-4">{question.answerCount}개의 답변</h2>

        {answers.length === 0 ? (
          <div className="bg-white rounded-lg shadow-sm p-8 text-center text-gray-500">
            아직 답변이 없습니다. 첫 번째 답변을 작성해보세요!
          </div>
        ) : (
          <div className="space-y-4">
            {answers.map((answer) => (
              <div
                key={answer.id}
                className={`bg-white rounded-lg shadow-sm p-4 ${
                  answer.isAccepted ? 'border-2 border-green-500' : ''
                }`}
              >
                <div className="flex gap-4">
                  {/* 투표 */}
                  <div className="flex-shrink-0 text-center">
                    <button
                      onClick={() => handleVoteAnswer(answer.id)}
                      className="p-1 hover:bg-gray-100 rounded transition"
                    >
                      ▲
                    </button>
                    <div className="text-lg font-bold text-gray-800">{answer.voteCount}</div>
                    {answer.isAccepted && (
                      <div className="text-green-600 text-2xl">✓</div>
                    )}
                    {canAccept && !question.acceptedAnswerId && !answer.isAccepted && (
                      <button
                        onClick={() => handleAcceptAnswer(answer.id)}
                        className="mt-2 text-sm text-gray-500 hover:text-green-600"
                      >
                        채택
                      </button>
                    )}
                  </div>

                  {/* 답변 내용 */}
                  <div className="flex-1">
                    <div className="prose max-w-none mb-4">
                      <div className="whitespace-pre-wrap">{answer.content}</div>
                    </div>
                    <div className="flex items-center gap-4 text-sm text-gray-500 pt-4 border-t">
                      <span className="font-medium text-gray-700">{answer.authorName}</span>
                      <span>{formatDate(answer.createdAt)}</span>
                      {answer.isAccepted && (
                        <span className="text-green-600 font-medium">✓ 채택된 답변</span>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* 답변 작성 */}
      <section className="bg-white rounded-lg shadow-sm p-6">
        <h2 className="text-lg font-semibold mb-4">답변 작성</h2>
        {isAuthenticated ? (
          <form onSubmit={handleAnswerSubmit}>
            <textarea
              value={newAnswer}
              onChange={(e) => setNewAnswer(e.target.value)}
              placeholder="답변을 입력하세요..."
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent resize-none"
              rows={6}
            />
            <div className="flex justify-end mt-2">
              <button
                type="submit"
                disabled={!newAnswer.trim() || isSubmitting}
                className="px-6 py-2 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 transition disabled:opacity-50"
              >
                {isSubmitting ? '등록 중...' : '답변 등록'}
              </button>
            </div>
          </form>
        ) : (
          <div className="p-4 bg-gray-50 rounded-lg text-center">
            <Link to="/login" className="text-green-600 hover:underline">
              로그인
            </Link>
            하여 답변을 작성하세요.
          </div>
        )}
      </section>

      {/* 목록 버튼 */}
      <div className="text-center">
        <Link
          to="/questions"
          className="inline-block px-6 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition"
        >
          목록으로
        </Link>
      </div>
    </div>
  );
}
